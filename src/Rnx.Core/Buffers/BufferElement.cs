﻿using Rnx.Abstractions.Buffers;
using Rnx.Abstractions.Util;
using Rnx.Core.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Rnx.Core.Buffers
{
    internal class BufferElement : IBufferElement
    {
        public IDataStore Data { get; private set; } = new DefaultDataStore();

        private string _text;
        private Func<Stream> _streamFactory;

        public BufferElement(Func<Stream> streamFactory)
        {
            _streamFactory = streamFactory;
        }

        public BufferElement(string text)
        {
            Text = text;
            _streamFactory = () => Stream.Null;
        }

        public TData Get<TData>()
        {
            return Data.Get<TData>();
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
                    // when a caller requests the text, we read all text from the
                    // current stream and then we dispose the current stream
                    using (var sr = new StreamReader(Stream))
                    {
                        /// by calling the Setter Text we set the text value and
                        /// we also reset the <see cref="_streamFactory"/>, so that
                        /// a subsequent call to <see cref="Stream"/> will give the right result
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
                    // put cursor to the beginning, so subsequent calls are able to read from the beginning
                    ms.Seek(0, SeekOrigin.Begin);

                    return ms;
                });
            }
        }

        public bool HasText => _text != null;

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
                ms.Seek(0, SeekOrigin.Begin);
                clone = new BufferElement(() => ms);
            }

            clone.Data = Data.Clone();

            return clone;
        }
    }
}