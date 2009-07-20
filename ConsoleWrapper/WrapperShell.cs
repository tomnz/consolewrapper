using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace ConsoleWrapper
{
    class WrapperShell : IWrapper, IWrapperListener
    {
        // Member variables
        private IList<IWrapperListener> _listeners;
        private IList<IWrapper> _wrappers;
        private StringBuilder _out;
        private IList<ConsoleString> _availableLines;
        private string _currentLine;
        private Thread _alertListeners;
        private Thread _alertListenersFinished;
        private Boolean _alerting = false;
        private Boolean _newAlerts = false;
        private bool _active = true;
        private ConsoleString.StringType _nextType = ConsoleString.StringType.Normal;
        private bool _hasNextType = false;
        private DirectoryInfo _currentDirectory;

        public DirectoryInfo CurrentDirectory
        {
            get { return _currentDirectory; }
        }

        public WrapperShell()
            : this (new DirectoryInfo(Directory.GetCurrentDirectory()))
        {
        }

        public WrapperShell(DirectoryInfo directory)
        {
            // Initialise members
            _listeners = new List<IWrapperListener>();
            _availableLines = new List<ConsoleString>();
            _out = new StringBuilder();
            _wrappers = new List<IWrapper>();
            _currentDirectory = directory;

            ShowDirectory();
        }

        private void ShowDirectory()
        {
            OutputAppend(_currentDirectory.FullName + "> ", ConsoleString.StringType.Normal);
        }

        #region Disposal Code

        // Horrible dispose code - C# FTL when it comes to destructors
        private bool _disposed = false;

        ~WrapperShell()
        {
            // Dispose unmanaged resources only
            Dispose(false);
        }

        public void Dispose()
        {
            // Dispose of the managed and unmanaged resources
            Dispose(true);

            // Tell the GC that the Finalize process no longer needs
            // to be run for this object.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposeManagedResources)
        {
            _active = false;

            // Process only if mananged and unmanaged resources have
            // not been disposed of.
            if (!_disposed)
            {
                if (disposeManagedResources)
                {
                    // Dispose managed resources
                    foreach (IWrapper wrapper in _wrappers)
                    {
                        wrapper.Dispose();
                    }
                }
                // Dispose unmanaged resources
                if (_alertListeners != null)
                {
                    if (_alertListeners.IsAlive)
                    {
                        if (!_alertListeners.Join(100))
                        {
                            _alertListeners.Abort();
                            _alertListeners.Join();
                        }
                    }
                    _alertListeners = null;
                }
                _disposed = true;
            }
        }

        #endregion dispose

        private void OutputAppend(string str)
        {
            OutputAppend(str, ConsoleString.StringType.Normal);
        }

        private void OutputAppend(string str, ConsoleString.StringType type)
        {
            lock (_availableLines)
            {
                str = _currentLine + str;

                string[] lines = str.Split(new string[] { Environment.NewLine, "\n", "\n\r" }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length - 1; i++)
                {
                    if (_hasNextType && type == ConsoleString.StringType.Normal)
                        _availableLines.Add(new ConsoleString(lines[i], _nextType));
                    else
                        _availableLines.Add(new ConsoleString(lines[i], type));

                    _hasNextType = false;

                }

                _currentLine = lines[lines.Length - 1];
            }

            StartAlertListeners();
        }

        private void StartAlertListeners()
        {
            // If there is already an alerting thread then
            // don't do anything
            _newAlerts = true;

            if (!_alerting)
            {
                // The following may not be absolutely necessary but
                // better safe than sorry :D
                if (_alertListeners != null)
                {
                    if (_alertListeners.IsAlive)
                    {
                        // Give thread some time to exit
                        if (!_alertListeners.Join(100))
                        {
                            _alertListeners.Abort();
                            _alertListeners.Join();
                        }
                    }
                    _alertListeners = null;
                }
                _alertListeners = new Thread(new ThreadStart(AlertListeners));
                _alerting = true;
                _alertListeners.Start();
            }
        }

        private void AlertListeners()
        {
            try
            {
                if (!_alerting)
                    _alerting = true;

                while (_newAlerts)
                {
                    _newAlerts = false;

                    // 40 ms wait time between alerts as text may
                    // be coming in in single character chunks
                    Thread.Sleep(40);

                    lock (_listeners)
                    {
                        foreach (IWrapperListener listener in _listeners)
                        {
                            listener.TextReady(this);
                        }
                    }
                }

                _alerting = false;
            }
            catch (ThreadAbortException)
            {
                // Don't really need to do any sort of cleanup here
                _alerting = false;
            }
        }

        private void AlertListenersFinished()
        {
            try
            {
                lock (_listeners)
                {
                    foreach (IWrapperListener listener in _listeners)
                    {
                        listener.WrapperFinished(this);
                    }
                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        private void ParseInput(String line, ConsoleString.StringType type)
        {
            if (_wrappers.Count > 0)
            {
                OutputAppend(line + Environment.NewLine, type);

                foreach (IWrapper wrapper in _wrappers)
                {
                    wrapper.SendLine(line, type);
                }
            }
            else
            {
                String[] args = line.Split(' ');

                switch (args[0])
                {
                    case "exit":
                        {
                            AlertListenersFinished();
                            return;
                        }
                    case "cd":
                        {
                            try
                            {
                                Directory.SetCurrentDirectory(args[1]);
                                _currentDirectory = new DirectoryInfo(args[1]);

                                OutputAppend(Environment.NewLine);
                            }
                            catch (DirectoryNotFoundException)
                            {
                                OutputAppend("Directory not found: " + args[1] + Environment.NewLine, ConsoleString.StringType.Err);
                            }
                        } break;
                    default:
                        {
                            OutputAppend(line + Environment.NewLine);

                            try
                            {
                                Wrapper wrapper = new Wrapper("cmd.exe", "/c " + line, _currentDirectory.FullName);
                                wrapper.AddListener(this);

                                _wrappers.Add(wrapper);
                            }
                            catch (Exception e)
                            {
                                OutputAppend(e.Message + Environment.NewLine, ConsoleString.StringType.Err);
                            }
                        } break;
                }

                ShowDirectory();
            }
        }

        #region IWrapper Members

        public void AddListener(IWrapperListener listener)
        {
            lock (_listeners)
            {
                _listeners.Add(listener);
            }
        }

        public ConsoleString[] GetText()
        {
            ConsoleString[] output;

            lock (_availableLines)
            {
                output = new ConsoleString[_availableLines.Count];
                
                _availableLines.CopyTo(output, 0);

                _availableLines.Clear();
            }

            return output;
        }

        public ConsoleString[] PeekText()
        {
            ConsoleString[] output;

            lock (_availableLines)
            {
                output = new ConsoleString[_availableLines.Count];

                _availableLines.CopyTo(output, 0);
            }

            return output;
        }

        public void SendLine(string line, ConsoleString.StringType type)
        {
            _nextType = type;
            _hasNextType = true;

            ParseInput(line, type);
        }

        public void SendCharacter(char character, bool flush)
        {
            foreach (IWrapper wrapper in _wrappers)
            {
                wrapper.SendCharacter(character, flush);
            }
        }

        public void Send(string text, bool flush)
        {
            foreach (IWrapper wrapper in _wrappers)
            {
                wrapper.Send(text, flush);
            }
        }

        public string GetCurrentLine()
        {
            return _currentLine;
        }

        #endregion

        #region IWrapperListener Members

        public void TextReady(IWrapper sender)
        {
            //_currentLine = new StringBuilder(_wrapper.GetCurrentLine());

            ConsoleString[] strings = sender.GetText();

            lock (_availableLines)
            {
                foreach (ConsoleString str in strings)
                {
                    _availableLines.Add(str);
                }
            }

            _currentLine = strings[strings.Length - 1].Text;

            StartAlertListeners();
        }

        public void WrapperFinished(IWrapper sender)
        {
            _wrappers.Remove(sender);

            if (_wrappers.Count == 0)
            {
                _currentLine = _currentDirectory.FullName + "> ";
            }
        }

        #endregion
    }
}
