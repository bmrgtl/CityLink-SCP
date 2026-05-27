using CityLink_SCP.DbModels;
using CityLink_SCP.Models;
using System.Xml.Serialization;

namespace CityLink_SCP.ViewModels;

public class Enquiry : IXmlViewModel
{
    public string PartialName => "_EnquiryCard";
    [XmlIgnore] public User? User { get; set; } = null;
    public string Heading { get; set; } = "More Questions?";
    public string SubHeading { get; set; } = "You can contact us at anytime.";

}

public class ContactUs : IXmlViewModel
{
    public string PartialName => "ContactUs";
    [XmlIgnore] public User? User { get; set; } = null;
    public string PageHeading { get; set; } = "Contact Us";
    public string PageSubHeading { get; set; } = "PHONE: (08) XXX XXX | EMAIL: INFO@CITYLINK.WA.GOV.AU";
    public string FormTitle { get; set; } = "Let us know your thoughts!";

}

public class BookEvent : IXmlViewModel
{
    public string PartialName => "BookEvent";
    [XmlIgnore] public User? User { get; set; }
    [XmlIgnore] public Event? Event { get; set; }
    [XmlIgnore] public EventRegistration? EventRegistration { get; set; }
    public string PageHeading { get; set; } = "Book Event";
}

public class BookService : IXmlViewModel
{
    public string PartialName => "BookService";
    [XmlIgnore] public User? User { get; set; }
    [XmlIgnore] public Service Service { get; set; } = new();
    [XmlIgnore] public ServiceBooking ServiceBooking { get; set; } = new();

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

    // Page text
    public string PageHeading { get; set; } = "Book a Service";
    public string FormHeading { get; set; } = "Book a Service";

}