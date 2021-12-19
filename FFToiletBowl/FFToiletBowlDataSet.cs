namespace FFToiletBowl
{
    
    
    public partial class FFToiletBowlDataSet {
        partial class InjuryReportDataTable
        {
        }
    }
}

namespace FFToiletBowl.FFToiletBowlDataSetTableAdapters
{
    partial class InjuryReportTableAdapter
    {
    }

    partial class InjuredReserveTableAdapter
    {
    }

    partial class ScheduleTableAdapter
    {
    }
    
    
    public partial class StatsTableAdapter {
        public int InsertCommandTimeout
        {
            get
            {
                return (this._adapter.InsertCommand.CommandTimeout);
            }

            set
            {
                this._adapter.InsertCommand.CommandTimeout = value;
            }
        }

        public int UpdateCommandTimeout
        {
            get
            {
                return (this._adapter.UpdateCommand.CommandTimeout);
            }

            set
            {
                this._adapter.UpdateCommand.CommandTimeout = value;
            }
        }

        public int DeleteCommandTimeout
        {
            get
            {
                return (this._adapter.DeleteCommand.CommandTimeout);
            }

            set
            {
                this._adapter.DeleteCommand.CommandTimeout = value;
            }
        }

        public int SelectCommandTimeout
        {
            get
            {
                return (this._commandCollection[0].CommandTimeout);
            }

            set
            {
                for (int i = 0; i < this._commandCollection.Length; i++)
                {
                    if ((this._commandCollection[i] != null))
                    {
                        ((System.Data.SqlClient.SqlCommand)
                         (this._commandCollection[i])).CommandTimeout = value;
                    }
                }
            }
        }

    }
}
