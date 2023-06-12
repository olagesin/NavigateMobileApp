//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace NFCProj.Models
//{  
//    public class GetLocaationDto

//    {
//        public string Id { get; set; }
//        public string LocationType { get; set; }
//        public double Longitude { get; set; }
//        public double Latitude { get; set; }
//        public string Name { get; set; }
//        public int SpaceCount { get; set; }
//    }

//    public class ErrorItemModel
//    {
//        public string Key { get; set; }
//        public List<string> ErrorMessages { get; set; }
//    }

//    public class GlobalResponse<T>
//    {
//        public T Data { get; set; }
//        public List<ErrorItemModel> Errors { get; set; }

//        public GlobalResponse()
//        {
//            Errors = new List<ErrorItemModel>();
//        }
//    }

public class AddTagDto
{
    public string SerialNumber { get; set; }
}

public class GetTagDto
{
    public string Id { get; set; }
    public string SerialNumber { get; set; }
}
//}
