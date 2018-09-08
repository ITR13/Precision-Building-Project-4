using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PBP4
{
    /// <summary>
    /// Helper class to make it easier to create XDataHolders
    /// </summary>
    public class XDataHolderFactory : IEnumerable
    {
        private XDataHolder _xDataHolder = new XDataHolder();
        public XDataHolder Create()
        {
            return _xDataHolder.Clone();
        }

        public void Add(string key, object data)
        {
            _xDataHolder.Write(key, data);
        }

        public IEnumerator GetEnumerator()
        {
            return _xDataHolder.ReadAll().GetEnumerator();
        }
    }
}
