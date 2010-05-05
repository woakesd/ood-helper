using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlServerCe;

namespace OodHelper.net
{
    [Svn("$Id: db.cs 17583 2010-05-02 17:23:57Z david $")]
    class Db
    {
        public Db(string sql)
        {
            mCon = new SqlCeConnection();
            mCon.ConnectionString = Properties.Settings.Default.OodHelperConnectionString;
            mCmd = new SqlCeCommand(sql, mCon);
        }

        public int ExecuteNonQuery(Hashtable p)
        {
            try
            {
                mCmd.Parameters.Clear();
                if (p != null)
                {
                    foreach (string k in p.Keys)
                    {
                        mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                    }
                }
                mCon.Open();
                return mCmd.ExecuteNonQuery();
            }
            finally
            {
                if (mCon.State != ConnectionState.Closed)
                    mCon.Close();
            }
        }

        private SqlCeDataAdapter mAdapt;
        private SqlCeConnection mCon;
        private SqlCeCommand mCmd;

        public Object GetScalar(Hashtable p)
        {
            DataTable d = new DataTable();
            mCmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                }
            }
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            mAdapt.Fill(d);
            mCon.Close();
            if (d.Rows.Count > 0)
                return d.Rows[0][0];
            else
                return DBNull.Value;
        }

        public Hashtable GetHashtable(Hashtable p)
        {
            DataTable d = new DataTable();
            mCmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                }
            }
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            mAdapt.Fill(d);
            mCon.Close();
            Hashtable h = new Hashtable();
            if (d.Rows.Count > 0)
            {
                foreach (DataColumn c in d.Columns)
                    h[c.ColumnName] = d.Rows[0][c];
            }
            return h;
        }

        public void Fill(DataTable d, Hashtable p)
        {
            mCmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                }
            }
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            mAdapt.Fill(d);
            mCon.Close();
        }

        public DataTable GetData(Hashtable p)
        {
            DataTable t = new DataTable();
            mCmd.Parameters.Clear();
            if (p != null)
            {
                foreach (string k in p.Keys)
                {
                    mCmd.Parameters.Add(new SqlCeParameter(k, p[k]));
                }
            }
            mAdapt = new SqlCeDataAdapter(mCmd);
            mCon.Open();
            mAdapt.Fill(t);
            mCon.Close();
            return t;
        }

        public int Commit(DataTable d)
        {
            SqlCeCommandBuilder cmb = new SqlCeCommandBuilder(mAdapt);
            mAdapt.DeleteCommand = cmb.GetDeleteCommand();
            mAdapt.InsertCommand = cmb.GetInsertCommand();
            mAdapt.UpdateCommand = cmb.GetUpdateCommand();
            return mAdapt.Update(d);
        }

        public void Dispose()
        {
            if (mCon != null) mCon.Dispose();
            if (mCmd != null) mCmd.Dispose();
            if (mAdapt != null) mAdapt.Dispose();
        }
    }
}
