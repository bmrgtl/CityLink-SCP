using CityLink_SCP.DbModels;

namespace CityLink_SCP.Models
{
    public class BookServiceViewModel
    {
        public Service Service { get; set; }
        public ServiceBooking ServiceBooking { get; set; }
        public User User { get; set; }

        private DateOnly _serviceDate;
        public DateOnly ServiceDate
        {
            get => _serviceDate;
            set { _serviceDate = value; ServiceBooking.Start_Time = _serviceDate.ToDateTime(_serviceTime); }
        }

        private TimeOnly _serviceTime;
        public TimeOnly ServiceTime
        {
            get => _serviceTime;
            set { _serviceTime = value; ServiceBooking.Start_Time = _serviceDate.ToDateTime(_serviceTime); }
        }
    }
}
