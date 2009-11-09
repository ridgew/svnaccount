using System;
using System.Xml;
using System.Xml.Serialization;


namespace SynUtil
{
    /// <summary>
    /// 同步选项配置
    /// </summary>
    [Serializable]
    public class SynOption
    {
        private SynMode _mode = SynMode.Mixed;

        [XmlAttribute]
        public SynMode Mode 
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public SynRule[] MatchRules { get; set; }

        public SynRule[] ExcludeRules { get; set; }
    }

    [Serializable]
    public enum SynMode : byte
    { 
        Match,
        Exclued,
        Mixed
    }

   



}
