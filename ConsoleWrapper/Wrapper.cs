using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data;
using System.Diagnostics;
using System.Threading;

namespace ConsoleWrapper
{
    class Wrapper : IWrapper
    {
        // Member variables
        private IList<IWrapperListener> _listeners;
        private Process _process;
        private StreamReader _stderr;
        private StreamReader _stdout;
        private StreamWriter _stdin;
        private StringBuilder _out;
        private IList<ConsoleString> _availableLines;
        private string _currentLine;
        private Thread _outReader;
        private Thread _errReader;
        private Thread _alertListeners;
        private Thread _alertListenersFinished;
        private Boolean _alerting;
        private bool _active = true;
        private ConsoleString.StringType _nextType = ConsoleString.StringType.Normal;
        private bool _hasNextType = false;

        public Wrapper(String appName)
        {
            // Initialise members
            _listeners = new List<IWrapperListener>();
            _availableLines = new List<ConsoleString>();
            _out = new StringBuilder();

            // Set up the process for starting
            _process = new Process();
            _process.StartInfo.FileName = appName;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardError = true;
            _process.StartInfo.RedirectStandardInput = true;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.CreateNoWindow = true;

            // Start the process and hook the input/output
            _process.Start();

            _stderr = _process.StandardError;
            _stdout = _process.StandardOutput;
            _stdin = _process.StandardInput;

            // Start the reader threads
            _outReader = new Thread(new ThreadStart(ReadStreamOut));
            _outReader.IsBackground = true;
            _outReader.Start();

            _errReader = new Thread(new ThreadStart(ReadStreamErr));
            _errReader.IsBackground = true;
            _errReader.Start();
        }

        #region Disposal Code

        // Horrible dispose code - C# FTL when it comes to destructors
        private bool _disposed = false;

        ~Wrapper()
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
                    if (_process != null)
                    {
                        _process.Close();
                        _process.Dispose();
                        _process = null;
                    }
                }
                // Dispose unmanaged resources
                if (_outReader != null)
                {
                    if (_outReader.IsAlive)
                    {
                        if (!_outReader.Join(100))
                        {
                            _outReader.Abort();
                        }
                    }
                    _outReader = null;
                }

                if (_errReader != null)
                {
                    if (_errReader.IsAlive)
                    {
                        if (!_errReader.Join(100))
                        {
                            _errReader.Abort();
                        }
                    }
                    _outReader = null;
                }

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

        private void OutputAppend(string str, ConsoleString.StringType type)
        {
            lock (_availableLines)
            {
                str = _currentLine + str;

                string[] lines = str.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length - 1; i++)
                {
                    if (_hasNextType)
                    {
                        _availableLines.Add(new ConsoleString(lines[i], _nextType));
                        _hasNextType = false;
                    }
                    else
                        _availableLines.Add(new ConsoleString(lines[i], type));
                }

                _currentLine = lines[lines.Length - 1];
            }

            // If there is already an alerting thread then
            // don't do anything
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
            if(!_alerting)
                _alerting = true;

            // 40 ms wait time between alerts as text may
            // be coming in in single character chunks
            Thread.Sleep(40);

            _alerting = false;

            lock (_listeners)
            {
                foreach (IWrapperListener listener in _listeners)
                {
                    listener.TextReady(this);
                }
            }
        }

        private void AlertListenersFinished()
        {
            lock (_listeners)
            {
                foreach (IWrapperListener listener in _listeners)
                {
                    listener.WrapperFinished();
                }
            }
        }

        // Threaded method
        public void ReadStreamOut()
        {
            lock(_stdout)
            {
                while (_active && !_stdout.EndOfStream)
                {
                    // This seems the most efficient way of getting text
                    // as I can't find a blocking stream read method
                    if (_active && _stdout.Peek() != -1)
                    {
                        StringBuilder outStr = new StringBuilder();
                        outStr.Append((char)_stdout.Read());

                        int startTime = Environment.TickCount;

                        while (_stdout.Peek() != -1)
                        {
                            outStr.Append((char)_stdout.Read());

                            // Once the buffer has filled sufficiently or
                            // after 40ms
                            if (outStr.Length > 100 || Environment.TickCount - startTime > 40)
                            {
                                OutputAppend(outStr.ToString(), ConsoleString.StringType.Out);
                                outStr = new StringBuilder();
                            }
                        }

                        OutputAppend(outStr.ToString(), ConsoleString.StringType.Out);
                    }
                    
                    //Thread.Sleep(50);
                }

                // Alert the listeners that we are finished
                if (_alertListenersFinished != null)
                {
                    if (_alertListenersFinished.IsAlive)
                    {
                        // Give thread some time to exit
                        if (!_alertListenersFinished.Join(100))
                        {
                            _alertListenersFinished.Abort();
                            _alertListenersFinished.Join();
                        }
                    }
                    _alertListenersFinished = null;
                }
                _alertListenersFinished = new Thread(new ThreadStart(AlertListenersFinished));
                _alertListenersFinished.Start();
                _alertListenersFinished.Join();
            }
        }

        // Threaded method
        public void ReadStreamErr()
        {
            lock (_stderr)
            {
                while (!_stderr.EndOfStream && _active)
                {
                    if (_stderr.Peek() != -1)
                    {
                        StringBuilder errStr = new StringBuilder();
                        errStr.Append((char)_stderr.Read());
                        
                        // Currently just dump everything in the same
                        // place as _stdout... Has some unexpected
                        // results at times
                        while (_stderr.Peek() != -1)
                        {
                            errStr.Append((char)_stderr.Read());
                        }

                        OutputAppend(errStr.ToString(), ConsoleString.StringType.Err);
                    }

                    Thread.Sleep(50);
                }
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
            lock (_availableLines)
            {
                ConsoleString[] output = new ConsoleString[_availableLines.Count];
                
                _availableLines.CopyTo(output, 0);

                _availableLines.Clear();
                return output;
            }
        }

        public ConsoleString[] PeekText()
        {
            lock (_availableLines)
            {
                ConsoleString[] output = new ConsoleString[_availableLines.Count];

                _availableLines.CopyTo(output, 0);

                return output;
            }
        }

        public void SendLine(string line, ConsoleString.StringType type)
        {
            _nextType = type;
            _hasNextType = true;
            _process.StandardInput.WriteLine(line);
        }

        public void SendCharacter(char character, bool flush)
        {
            _process.StandardInput.Write(character);

            if (flush)
                _process.StandardInput.Flush();
        }

        public void Send(string text, bool flush)
        {
            _process.StandardInput.Write(text);

            if (flush)
                _process.StandardInput.Flush();
        }

        public string GetCurrentLine()
        {
            return _currentLine;
        }

        #endregion
    }
}
