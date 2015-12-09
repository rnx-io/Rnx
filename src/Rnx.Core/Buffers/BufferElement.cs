using Rnx.Common.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rnx.Core.Buffers
{
    internal class BufferElement : IBufferElement
    {
        private string _text;
        private Func<Stream> _streamFactory;

        private BufferElement()
        {
            Data = new BufferElementData();
        }

        public BufferElement(Func<Stream> streamFactory) : this()
        {
            _streamFactory = streamFactory;
        }

        public BufferElement(string text) : this()
        {
            Text = text;
            _streamFactory = () => Stream.Null;
        }

        public Stream Stream
        {
            get { return _streamFactory(); }
            set
            {
                _streamFactory = () => value ?? Stream.Null;
                _text = null;
            }
        }

        public string Text
        {
            get
            {
                if (_text == null)
                {
                    using (var sr = new StreamReader(Stream))
                    {
                        Text = sr.ReadToEnd();
                    }
                }

                return _text;
            }
            set
            {
                _text = value ?? "";

                _streamFactory = new Func<Stream>(() =>
                {
                    var bytes = Encoding.UTF8.GetBytes(_text);
                    var ms = new MemoryStream();
                    ms.Write(bytes, 0, bytes.Length);

                    return ms;
                });
            }
        }

        public bool HasText => _text != null;

        public IBufferElementData Data { get; private set; }

        public IBufferElement Clone()
        {
            BufferElement clone = null;

            if (HasText)
            {
                clone = new BufferElement(_text);
            }
            else
            {
                var ms = new MemoryStream();
                Stream.CopyTo(ms);
                clone = new BufferElement(() => ms);
            }

            clone.Data = (IBufferElementData)Data.Clone();

            return clone;
        }
    }
}