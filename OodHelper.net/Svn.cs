using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OodHelper.net
{
    /// <summary>
    /// Summary description for EKISSVNAttribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    [Svn("$Id: Svn.cs 17588 2010-05-04 19:37:30Z david $")]
    public class Svn : System.Attribute
    {
        private string keyword;

        public Svn(string KeywordValue)
        {
            keyword = KeywordValue;
        }

        /// <summary>
        /// Define SVN Keyword property.
        /// This is a read-only attribute.
        /// </summary>
        public virtual string Keyword
        {
            get { return keyword; }
        }

        public static int Revision()
        {
            int revision = 0;
            System.Reflection.Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
            Type[] modules = ass.GetTypes();
            foreach (Type m in modules)
            {
                Svn[] attribs = (Svn[]) m.GetCustomAttributes(typeof(Svn), false);
                foreach (Svn attrib in attribs)
                {
                    string[] v = attrib.Keyword.Split(new char[] { ' ' });
                    int t;
                    if (v.Length >= 2)
                    {
                        if (Int32.TryParse(v[2], out t))
                            revision = Math.Max(revision, t);
                    }
                }
            }
            return revision;
        }
    }
}
