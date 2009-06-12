using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using System.IO;

namespace ConsoleWrapper
{
    public class ConsoleString
    {
        public enum StringType
        {
            Normal,
            Out,
            Err,
            Input,
            Special
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        private bool _finalised = true;
        public bool Finalised
        {
            get { return _finalised; }
            set { _finalised = value; }
        }

        private StringType _type = StringType.Normal;
        public StringType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        private bool _colorSet = false;
        private Color _color;
        public Color Color
        {
            get 
            {
                if (_colorSet)
                {
                    return _color;
                }
                else
                {
                    switch (_type)
                    {
                        case ConsoleString.StringType.Err:
                            {
                                return Color.Orange;
                            }
                        case ConsoleString.StringType.Input:
                            {
                                return Color.LightGreen;
                            }
                        case ConsoleString.StringType.Special:
                            {
                                return Color.Aqua;
                            }
                        default:
                            {
                                return Color.White;
                            }
                    }
                }
            }
            set { _colorSet = true; _color = value; }
        }

        public ConsoleString(String text)
        {
            _text = text;
        }

        public ConsoleString(String text, StringType type)
            :
            this(text)
        {
            _type = type;
        }

        public ConsoleString(String text, Color color)
            :
            this(text)
        {
            _colorSet = true;
            _color = color;
        }

        public ConsoleString(String text, StringType type, bool finalised)
            :
            this(text, type)
        {
            _finalised = finalised;
        }
    }
}
