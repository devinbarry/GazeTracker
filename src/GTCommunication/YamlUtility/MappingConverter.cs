using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QiHe.Yaml.Grammar;

namespace GTCommunication.YamlUtility
{
    public class MappingConverter
    {
        public enum KeyCase
        {
            FORCE_UPPER,
            FORCE_LOWER,
            UNCHANGED
        }

        public static Dictionary<String, Int32> convertToDictionary( Mapping m, KeyCase forceKeyCase )
        {
            Dictionary<String, Int32> dic = new Dictionary<string, Int32>();
            foreach ( MappingEntry me in m.Enties )
            {
                String key = me.Key.ToString();
                Int32 value = Convert.ToInt32( Double.Parse( me.Value.ToString() ) );
                KeyValuePair<String, Int32> kvp = makeKvp( key, value, forceKeyCase );
                dic.Add( kvp.Key, kvp.Value );
            }

            return dic;
        }

        private static KeyValuePair<string, int> makeKvp( string key, int value, KeyCase forceKeyCase )
        {
            if ( forceKeyCase.CompareTo( KeyCase.FORCE_UPPER ) == 0 )
            {
                key = key.ToUpper();
            }
            if ( forceKeyCase.CompareTo( KeyCase.FORCE_LOWER ) == 0 )
            {
                key = key.ToLower();
            }
            return new KeyValuePair<string, int>( key, value );
        }
    }
}
