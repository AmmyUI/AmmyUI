using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace AmmySidekick
{
    class ListenerParser
    {
        public event EventHandler<ListenerParserEventArgs> MessageReceived;

        private enum ParserState
        {
            Init,
            Header,
            NumberOfMessages,
            TargetIdLen,
            TargetId,
            MessageLen,
            Message,
            Checksum,
            PropertiesLength,
            PropertyList,
            Footer
        };
        
        private readonly List<byte> _buffer = new List<byte>();
        private ParserState _parserState = ParserState.Init;
        private int _targetIdLen;
        private string _targetId;
        private int _messageLen;
        private byte[] _message;
        private ushort _expectedChecksum;
        private byte _numberOfMessages;
        private List<Message> _messages;
        private ushort _propertiesLength;

        public void Feed(byte b)
        {
            switch (_parserState) {
                case ParserState.Init:
                    if (b == 0xbe) {
                        _messages = new List<Message>();
                        _parserState = ParserState.Header;
                        //Debug.WriteLine("init-");
                    }
                    break;
                case ParserState.Header:
                    if (b == 0xef) {
                        _parserState = ParserState.NumberOfMessages;
                        //Debug.WriteLine("header-");
                    }
                    break;
                case ParserState.NumberOfMessages:
                    _numberOfMessages = b;
                    _parserState = ParserState.TargetIdLen;
                    //Debug.WriteLine("number(" + _numberOfMessages + ")-");
                    break;
                case ParserState.TargetIdLen:
                    _buffer.Add(b);

                    if (_buffer.Count == 4) {
                        _targetIdLen = BitConverter.ToInt32(_buffer.ToArray(), 0);
                        _buffer.Clear();
                        
                        if (_targetIdLen <= 0 || _targetIdLen > 10000)
                            _parserState = ParserState.Init;
                        else
                            _parserState = ParserState.TargetId;
                        //Debug.WriteLine("targetidlen(" + _targetIdLen + ")-");
                    }
                    break;
                case ParserState.TargetId:
                    _buffer.Add(b);

                    if (_buffer.Count == _targetIdLen) {
                        _targetId = Encoding.Unicode.GetString(_buffer.ToArray());
                        _buffer.Clear();
                        _parserState = ParserState.MessageLen;
                        //Debug.WriteLine("targetid(" + _targetId + ")-");
                    }
                    break;
                case ParserState.MessageLen:
                    _buffer.Add(b);

                    if (_buffer.Count == 4) {
                        _messageLen = BitConverter.ToInt32(_buffer.Take(4).ToArray(), 0);
                        _buffer.Clear();
                        _parserState = ParserState.Message;

                        if (_messageLen <= 0 || _messageLen > 1024*1024) {
                            _parserState = ParserState.Init;
                            //Debug.WriteLine("invalid-messagelen(" + _messageLen + ")-");
                        } else {
                            _parserState = ParserState.Message;
                            //Debug.WriteLine("messagelen(" + _messageLen + ")-");
                        }
                    }
                    break;
                case ParserState.Message:
                    _buffer.Add(b);

                    if (_buffer.Count == _messageLen) { 
                        var bufferArray = _buffer.ToArray();

                        _message = bufferArray;
                        _expectedChecksum = Fletcher16(bufferArray);
                        _buffer.Clear();
                        _parserState = ParserState.Checksum;
                        //Debug.WriteLine("message(" + _message + ")-");
                    }
                    break;
                case ParserState.Checksum:
                    _buffer.Add(b);

                    if (_buffer.Count == 2) {
                        var checksum = BitConverter.ToUInt16(_buffer.ToArray(), 0);
                        _buffer.Clear();

                        if (_expectedChecksum == checksum) {
                            _messages.Add(new Message {
                                TargetId = _targetId,
                                Buffer = _message
                            });
                            
                            _parserState = ParserState.PropertiesLength;
                            //Debug.WriteLine("checksum-");
                        }
                        else 
                            _parserState = ParserState.Init;
                    }
                    break;
                case ParserState.PropertiesLength:
                    _buffer.Add(b);

                    if (_buffer.Count == 2) {
                        _propertiesLength = BitConverter.ToUInt16(_buffer.ToArray(), 0);
                        _buffer.Clear();

                        if (_propertiesLength > 0)
                            _parserState = ParserState.PropertyList;
                        else if (_numberOfMessages > 1) {
                            _numberOfMessages--;
                            _parserState = ParserState.TargetIdLen;
                        }
                        else
                            _parserState = ParserState.Footer;
                        //Debug.WriteLine("proplen(" + _propertiesLength + ")-");
                    }

                    break;
                case ParserState.PropertyList:
                    _buffer.Add(b);

                    if (_buffer.Count == _propertiesLength) {
                        _messages.Last().PropertyList = Encoding.Unicode.GetString(_buffer.ToArray());
                        _buffer.Clear();
                        
                        if (--_numberOfMessages > 0)
                            _parserState = ParserState.TargetIdLen;
                        else
                            _parserState = ParserState.Footer;

                        //Debug.WriteLine("proplst-");
                    }
                    break;
                case ParserState.Footer:
                    if (b == 0xff) {
                        var evt = MessageReceived;
                        if (evt != null)
                            evt(this, new ListenerParserEventArgs {
                                Messages = _messages
                            });

                        //Debug.WriteLine("footer");
                    }

                    _parserState = ParserState.Init;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ushort Fletcher16(byte[] data)
        {
            ushort sum1 = 0;
            ushort sum2 = 0;
            
            for(var index = 0; index<data.Length; ++index )
            {
                sum1 = (ushort) ((sum1 + data[index]) % 255);
                sum2 = (ushort) ((sum2 + sum1) % 255);
            }
            
            return (ushort) ((sum2 << 8) | sum1);
        }
    }

    public class Message
    {
        public string TargetId { get; set; }
        public byte[] Buffer { get; set; }
        public string PropertyList { get; set; }
    }
}