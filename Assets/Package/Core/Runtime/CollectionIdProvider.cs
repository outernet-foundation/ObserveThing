using System;

namespace ObserveThing
{
    public class CollectionIdProvider
    {
        private Func<uint, bool> _validateId;
        private bool _validateBeforeFirstLoop;
        private uint _nextId = 0;
        private bool _looped;

        public CollectionIdProvider(Func<uint, bool> validateId, bool validateBeforeFirstLoop = false)
        {
            _validateId = validateId;
            _validateBeforeFirstLoop = validateBeforeFirstLoop;
        }

        public uint GetUnusedId()
        {
            uint id = _nextId;

            if (_looped || _validateBeforeFirstLoop)
            {
                bool found = false;

                for (uint i = _nextId; i < uint.MaxValue; i++)
                {
                    if (_validateId(i))
                    {
                        id = i;
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    for (uint i = 0; i < _nextId; i++)
                    {
                        if (_validateId(i))
                        {
                            id = i;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        throw new Exception("No valid ID found!");
                }
            }

            if (id == uint.MaxValue)
            {
                _nextId = 0;
                _looped = true;
            }
            else
            {
                _nextId = id + 1;
            }

            return id;
        }

        public void Reset()
        {
            _looped = false;
            _nextId = 0;
        }
    }
}