using GMS.Models.Entities.Collections;
using GMS.Models.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GMS.Models.Service
{
    public static class Service
    {
        private static TreeRepository treeRepos = new TreeRepository();
        private static GlassRepository glassRepos = new GlassRepository();
        private static EmployeeRepository emplRepos = new EmployeeRepository();
        private static AreaRepository areaRepos = new AreaRepository();
        private static RegistrationRepository regisRepos = new RegistrationRepository();
        private static SectionRepository secRepos = new SectionRepository();
        private static UnitRepository unitRepos = new UnitRepository();
        private static Random random = new Random();
        

        private static string[] families = {
            "Bách",
            "Dầu",
            "Sim",
            "Tú Cầu",
            "Xoan",
            "Côm",
            "Gai Dầu",
            "Ô Rô",
            "Dương Xỉ"
        };
        private static string[] trees = {
            "Cây Dừa",
            "Cây Bàng",
            "Cây Ổi",
            "Cây Cóc",
            "Cây Mận",
            "Cây Đào",
            "Cây Tre",
            "Cây Cau",
            "Cây Phượng",
            "Cây Đu Đủ",
            "Cây Xoài",
            "Cây Cherry",
            "Cây Cam",
            "Cây Quýt",
            "Cây Bưởi",
            "Cây Bạch Đàn",
            "Cây Đa",
            "Cây Hồ Điệp",
            "Cây Chanh",
            "Cây Móng Bò",
            "Cây Lim",
            "Cây Bằng Lăng",
            "Cây Sò",
            "Cây Ngọc Lan",
            "Cây Sanh",
            "Cây Sao Đen",
            "Cây Hoa Sữa",
            "Cây Liễu",
            "Cây Osaka",
            "Cây Viết",
            "Cây Tùng Tháp",
            "Cây Tường Vi",
            "Cây Hoa Giấy",
            "Cây Ngọc",
            "Cây Ắc Ó",
            "Cây Long Não",
            "Cây Thầu Dầu",
            "Cây Khế",
            "Cây Me",           
        };
        private static string[] status = {"Khỏe", "Yếu","Bệnh","Chết"};
        private static string[] types = {
            "Thân Gỗ"
        };
        private static string[] gender = { "Nam", "Nữ" };
        private static string[] position = {
            "Nhân viên",
            "Nhân Viên",
            "Nhân Viên",
            "Nhân Viên",
            "Nhân Viên",
            "Nhân Viên",
            "Nhân Viên",
            "Quản lý"
        };
        private static string[] firstNameMale = {
            "Hùng",
            "Dũng",
            "Toàn",
            "Lý",
            "Kiệt",
            "Khánh",
            "Vinh",
            "Cường",
            "Nam",
            "Sơn",
            "Phúc",
            "Thành",
            "Bùi",
            "Đức",
            "Tài",
            "Tân",
            "Hồ",
            "Duy",
            "Mạnh",
            "Châu",
            "Vỹ",
            "Tín",
            "Hữu",
            "Đoàn",
            "Luyện",
            "Khá",
            "Phú",
            "Ân",
            "Kha",
            "Trung",
            "Tâm",
            "Minh",
            "Thuận",
            "Trường",
            "Bảo",
            "Long",
            "Phong",
            "Phi",
            "Bình",
            "Lộc",
            "Minh",
            "Chinh"
        };
        private static string[] firstNameFemale = {
            "Hoa",
            "Quỳnh",
            "Liễu",
            "Như",
            "Linh",
            "Phương",
            "Huệ",
            "Lan",
            "Ngọc",
            "Anh",
            "Hương",
            "Lệ",
            "Tâm",
            "Thanh",
            "Hà",
            "Ngân",
            "Liên",
            "Hạ",
            "Châu",
            "Kiều",
            "Hạnh",
            "Trúc",
            "Trân",
            "My",
            "Thy",
            "Nga",
            "Phụng",
            "Vy",
            "Mai",
            "Thảo",
            "Trang",
            "Tuyến",
            "Uyên",
            "Duyên",
            "Hằng",
            "Điệp"
        };
        private static string[] midNameMale = {
            "Anh",
            "Đình",
            "Đinh",
            "Đoàn",
            "Hoài",
            "",
            "",
            "",
            "",
            "",
            "Chí",
            "Chiến",
            "Đăng",
            "Đức",
            "Gia",
            "Hữu",
            "Hùng",
            "Hải",
            "Hoàng",
            "Khôi",
            "Minh",
            "Ngọc",
            "Quốc",
            "Tài",
            "Thái",
            "Thành",
            "Thiên",
            "Thanh",
            "Trọng",
            "Văn",
            "Xuân",
            "Trung",
            "Tiến",
            "Quang",
            "",
            "",
            "",
            "",
            "Bá",
            "Bảo",
            "Cao",
            "Công",
            "Khắc",
            "Minh",
            "Nhật",
            "Quang",
            "Quốc",
            "Sỹ",
            "Thế",
            "Thiện",
            "Việt",
            "Xuân"
        };
        private static string[] midNameFemale = {
            "Ái",
            "Ánh",
            "Thị",
            "Thị Bạch",
            "Bạch",
            "Băng",
            "Bảo",
            "Bích",
            "Cẩm",
            "Cát",
            "Dạ",
            "Đan",
            "Diễm",
            "Diệu",
            "Đoan",
            "Đan",
            "Gia",
            "Nhã",
            "Mỹ",
            "Mai",
            "Giáng",
            "Hạ",
            "Hải",
            "Hạnh",
            "Hoài",
            "Hoàng",
            "Hồng",
            "Huệ",
            "Hương",
            "Huyền",
            "Khánh",
            "Kiều",
            "Kim",
            "Lam",
            "Lan",
            "Lệ",
            "Linh",
            "Minh",
            "Ngọc"
        };
        private static string[] lastName = {
            "Nguyễn",
            "Lê",
            "Trần",
            "Phạm",
            "Hoàng",
            "Huỳnh",
            "Phan",
            "Vũ",
            "Võ",
            "Đặng",
            "Bùi",
            "Đỗ",
            "Hồ",
            "Ngô",
            "Dương",
            "Lý",
            "Phạm",
            "Nguyễn",
            "Nguyễn",
            "Phạm",
            "Cao",
            "Tống",
            "Mạc",
            "Sầm",
            "Ung",
            "Vạn",
            "Đoàn"
        };
        private static string[] address = {
            "Bình Thuận",
            "Cà Mau",
            "Kiên Giang",
            "Hà Nội",
            "Phú Thọ",
            "Nha Trang",
            "Khánh Hòa",
            "Vũng Tàu",
            "Bắc Ninh",
            "Sài Gòn",
            "Bình Dương",
            "Bình Phước",
            "Bình Định",
            "Quảng Ninh",
            "Bắc Giang",
            "Mỹ Tho",
            "Long An",
            "Cần Thơ",
            "Quảng Nam",
            "Quảng Ngãi",
            "Thanh Hóa",
            "Hà Tĩnh",
            "Nghệ An",
            "Quảng Trị",
            "Hải Phòng",
            "Lâm Đồng",
            "Gia Lai",
            "Phú Yên",
            "Lai Châu",
            "Quảng Bình",
            "Đồng Nai",           
        };
        
        private static Dictionary<string, string> setArea = new Dictionary<string, string>();
        
        public static async Task Unit()
        {
            var becamex = new ManagedUnit();
            becamex.Id = "BIC";
            becamex.Name = "Becamex IDC";
            var eiu = new ManagedUnit();
            eiu.Id = "EIU";
            eiu.Name = "Eastern International University";
            await unitRepos.Create(becamex);
            await unitRepos.Create(eiu);
        }

        public static async Task Area()
        {
            setArea.Add("SE1", "Vòng Xuyến Lớn Trung Tâm");
            setArea.Add("SE2", "Vòng Xuyến Nhỏ Phụ");
            setArea.Add("SE3", "Quảng Trường");
            setArea.Add("SE4", "Bãi Trước B8-B11");
            setArea.Add("SE5", "Bãi Trước B4-B5");
            setArea.Add("SE6", "Bãi Trước B3-BBi");
            setArea.Add("SE7", "Bãi Trước Kề Sau BBi");
            setArea.Add("SE8", "Bãi Sau B11-VGU");
            setArea.Add("SE9", "Rìa Cổng Đông Bắc");
            setArea.Add("SE10", "Rìa Cồng Tây Bắc");
            setArea.Add("SE11", "Rìa Cổng Nam");
            setArea.Add("SE12", "Luống 1");
            setArea.Add("SE13", "Luống 2");
            setArea.Add("SE14", "Luống 3");
            setArea.Add("SE15", "Luống 4");
            setArea.Add("SE16", "Luống 5");
            setArea.Add("SE17", "Luống 6");
            Area bd = new Area();
            bd.Id = "PBD";
            bd.Name = "Bình Dương";
            bd.ManagedUnitId = "BIC";
            await areaRepos.AddNewNode(bd);
            Area tdm = new Area();
            tdm.Id = "DTDM";
            tdm.Name = "Thủ Dầu Một";
            tdm.ParentAreaId = "PBD";
            tdm.ManagedUnitId = "BIC";
            await areaRepos.AddNewNode(tdm);
            Area dh = new Area();
            dh.Id = "WDH";
            dh.Name = "Định Hòa";
            dh.ParentAreaId = "DTDM";
            dh.ManagedUnitId = "BIC";
            await areaRepos.AddNewNode(dh);
            Area eiu = new Area();
            eiu.Id = "UEIU";
            eiu.Name = "Eastern International University";
            eiu.ParentAreaId = "WDH";
            eiu.ManagedUnitId = "EIU";
            await areaRepos.AddNewNode(eiu);
            Regex reg = new Regex(@"\d+.\d+");
            Regex regId = new Regex(@"'(\w+)'");
            string id = "SE1";
            string[] text = File.ReadAllLines(@"C:\Users\DELL\Desktop\GMS Project\Polygon.txt");
            List<Coordinate> list = new List<Coordinate>();
            foreach (string line in text)
            {
                
                Match match = reg.Match(line);
                Match matchId = regId.Match(line);
                if (match.Success)
                {
                    string tempId = matchId.Groups[1].Value;
                    string lat = match.ToString();
                    string lng = match.NextMatch().ToString();
                    if (tempId == id)
                    {
                        list.Add(new Coordinate(lat, lng));
                    }
                    else
                    {
                        Polygon polygon = new Polygon(list);
                        Area area = new Area();
                        area.Id = id;
                        area.Name = setArea[area.Id];
                        area.Polygon = polygon;
                        area.ParentAreaId = "UEIU";
                        area.ManagedUnitId = "EIU";
                        await areaRepos.AddNewNode(area);
                        await areaRepos.AddLeafNodeReferenceRecursively(area.ParentAreaId, area.Id);
                        await secRepos.Create(new SectionData(area.Id));
                        id = tempId;
                        list = new List<Coordinate>();
                        list.Add(new Coordinate(lat, lng));
                    }                  
                }
            }
            Polygon pol = new Polygon(list);
            Area a = new Area();
            a.Id = id;
            a.Name = setArea[a.Id];
            a.Polygon = pol;
            a.ParentAreaId = "UEIU";
            a.ManagedUnitId = "EIU";
            await areaRepos.AddNewNode(a);
            await areaRepos.AddLeafNodeReferenceRecursively(a.ParentAreaId, a.Id);
            await secRepos.Create(new SectionData(a.Id));
        }

        public static Employee NewEmployee()
        {
            string gen = RandomArray(gender);
            string name = "";
            if(gen == "Nam")
            {
                string mid = RandomArray(midNameMale);
                if(mid != "")
                    name = RandomArray(lastName) + " " + mid + " " + RandomArray(firstNameMale);
                else
                    name = RandomArray(lastName) + " " + RandomArray(firstNameMale);
            }
            else
            {
                name = RandomArray(lastName) + " " + RandomArray(midNameFemale) + " " + RandomArray(firstNameFemale);
            }
  
            string add = RandomArray(address);
            string phone = RandomStringNumber(11);
            string pos = RandomArray(position);
            Employee emp = new Employee();
            emp.Name = name;
            emp.Phone = phone;
            emp.Gender = gen;
            emp.Address = add;
            emp.Position = pos;
            return emp;
        }

        public static async Task GenerateEmployee(int n)
        {
            for(int i = 1; i <= n; ++i)
            {
                Employee emp = NewEmployee();
                await emplRepos.Create(emp);
            }
        }

        public static async Task GeneratePlants(int n)
        {
            for (int i = 1; i <= n; ++i)
            {
                Plant plant = null;
                if (plant.Type == "Thân gỗ")
                {
                   await treeRepos.Create((Tree)plant);
                }
                else
                {
                   await glassRepos.Create((Glass)plant);
                }
            }
        }

        public static async Task GenerateTree()
        {
            List<Employee> list = await emplRepos.FindAll();
            string[] ids = new string[list.Count()];
            int t = 0;
            foreach(var e in list)
            {
                ids[t++] = e.Id;
            }
            Regex reg = new Regex(@"(\d+.\d+)");
            Regex regId = new Regex(@"'(\w+)'");
            string[] text = File.ReadAllLines(@"C:\Users\DELL\Desktop\GMS Project\Coord.txt");
            foreach(string line in text)
            {
                Match match = reg.Match(line);
                Match matchId = regId.Match(line);
                if (match.Success)
                {
                    string id = matchId.Groups[1].Value;
                    string lag = reg.Matches(line)[0].ToString();
                    string lng = reg.Matches(line)[1].ToString();
                    Coordinate coord = new Coordinate(lag, lng);
                    Plant plant = NewPlant(coord, ids, id);
                    await treeRepos.Create((Tree)plant);
                    await secRepos.AddTreeToSection(plant.Location.SectionId, plant.Id);
                }
            }
        }

        public static async Task<Boolean> GenerateRegisInfo()
        {
            await regisRepos.Create(RegisInfo());
            return true;
        }

        public static Registration RegisInfo()
        {
            Registration regis = new Registration();
            regis.UserId = "3abb00e5-de54-4b1b-b2e4-80e10201dead";
            regis.SubmitDate = DateTime.Now;
            regis.RegisTree = (Tree)NewPlant(new Coordinate("11.054054114987355", "106.66664106878659"), null, null);
            return regis;
        }

        public static Plant NewPlant(Coordinate coord, string[] ids, string id)
        {
            string st = RandomArray(status);
            string type = RandomArray(types);
            string name = RandomArray(trees);
            string family = RandomArray(families);
            DateTime date = DateTime.Now;      
            if(type == "Thân Gỗ")
            {
                
                double height = GetRandomNumber(1, 5);
                double width = GetRandomNumber(1, 5);
                string lat = GetRandomNumber(1, 20).ToString();
                string lng = GetRandomNumber(1, 20).ToString();
                //string empId = RandomArray(ids);
                Tree tree = new Tree();
                tree.Type = type;
                tree.Name = name;
                tree.Family = family;
                //tree.RegisDate = date;
                tree.Status = st;
                tree.Height = height;
                tree.Width = width;
                tree.Coord = coord;
                //tree.EmployeeId = empId;
                //tree.ManagedUnitId = "EIU";
                tree.Location = new Location("PBD", "DTDM", "WDH", "UEIU", "SE4");
                return tree;
            }
            double surface = GetRandomNumber(1, 10);
            Glass g = new Glass();
            g.Type = type;
            g.Name = name;
            g.Family = family;
            g.Status = st;
            g.RegisDate = date;
            g.SurfaceArea = surface;
            List<Coordinate> list = new List<Coordinate>();
            for(int i = 1; i <= 10; i++)
            {
                string lat = GetRandomNumber(1, 20).ToString();
                string lng = GetRandomNumber(1, 20).ToString();
                Coordinate po = new Coordinate(lat, lng);
                list.Add(po);

            }
            g.Polygon = new Polygon(list);
            return g;
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZqwertyuiopasdfghjklzxcvbnm";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomStringNumber(int length)
        {
            const string chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string RandomArray(string[] arr)
        {
            int index = random.Next(0, arr.Length);
            return arr[index];
        }

        public static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        //Service

        /*<--Area Tree Relationship-->*/
        public static async Task AssignNode(Area node)
        {
            if (node.Id.StartsWith("S"))
            {
                await AssignBaseNode(node);
            }
            else
            {
                await AssignNormalNode(node);
            }
        }

        public static async Task AssignNormalNode(Area normalNode)
        {
            //assign child node
           foreach(var id in normalNode.ChildAreaIds)
           {
                var childNode = await areaRepos.Find(id);
                childNode.ParentAreaId = normalNode.Id;
                if (childNode.Id.StartsWith("S"))
                    normalNode.leavesIds.Add(childNode.Id);
                else
                    normalNode.leavesIds.AddRange(childNode.leavesIds);
                await areaRepos.Update(childNode);
           }
           await areaRepos.Update(normalNode);
           await areaRepos.AddLeafNodeReferenceRecursively(normalNode.ParentAreaId, normalNode.leavesIds);
        }

        public static async Task AssignBaseNode(Area baseNode)
        {
            await areaRepos.AddLeafNodeReferenceRecursively(baseNode.ParentAreaId, baseNode.Id);
            await secRepos.Create(new SectionData(baseNode.Id));
        }

        public static async Task UnassignNode(Area node)
        {
            if (node.Id.StartsWith("S"))
            {
                await ClearAllPlantsInABaseStorage(node);
                await UnassignBaseNode(node);
            }
            else
            {
                await UnassignNormalNode(node);
            }
        }

        public static async Task UnassignNormalNode(Area normalNode)
        {
            foreach(var id in normalNode.ChildAreaIds)
            {
                var childNode = await areaRepos.Find(id);
                childNode.ParentAreaId = null;
                await areaRepos.Update(childNode);
            }
            await areaRepos.RemoveLeafNodeReferenceRecursively(normalNode.ParentAreaId, normalNode.leavesIds);
        }

        public static async Task UnassignBaseNode(Area baseNode)
        {
            await areaRepos.RemoveLeafNodeReferenceRecursively(baseNode.ParentAreaId, baseNode.Id);
            await secRepos.Delete(baseNode.Id);
        }

        public static async Task ClearAllPlantsInABaseStorage(Area baseNode)
        {
            var baseStorage = await secRepos.Find(baseNode.Id);
            foreach (var id in baseStorage.TreeIds)
            {
                await treeRepos.Delete(id);
            }
            foreach (var id in baseStorage.glassIds)
            {
                await glassRepos.Delete(id);
            }
        }

        public static async Task ModifyParentNode(Area currentNode, Area updateNode)
        {
           await ModifyRelationship(currentNode, updateNode);
           await RemoveNodeReferences(currentNode);
        }

        public static async Task ModifyRelationship(Area currentNode, Area updateNode)
        {
            var newParent = await areaRepos.Find(updateNode.ParentAreaId);
            newParent.ChildAreaIds.Add(updateNode.Id);
            await areaRepos.Update(newParent);
            var oldParent = await areaRepos.Find(currentNode.ParentAreaId);
            if(oldParent != null)
            {
                oldParent.ChildAreaIds.Remove(currentNode.Id);
                await areaRepos.Update(oldParent);
            }
        }

        public static async Task RemoveNodeReferences(Area node)
        {
            if (node.Id.StartsWith("S"))
            {
                await areaRepos.RemoveLeafNodeReferenceRecursively(node.ParentAreaId, node.Id);
            }
            else
            {
                await areaRepos.RemoveLeafNodeReferenceRecursively(node.ParentAreaId, node.leavesIds);
            }
        }

        public static async Task AddNodeReferences(Area node)
        {
            if (node.Id.StartsWith("S"))
            {
                await areaRepos.AddLeafNodeReferenceRecursively(node.ParentAreaId, node.Id);
            }
            else
            {
                await areaRepos.AddLeafNodeReferenceRecursively(node.ParentAreaId, node.leavesIds);
            }
        }

        public static bool IsLeavesModified(Area currentNode, Area updateNode)
        {
            var check1 = currentNode.leavesIds.Except(updateNode.leavesIds).ToList();
            var check2 = updateNode.leavesIds.Except(currentNode.leavesIds).ToList();
            if(check1.Any() || check2.Any())
            {
                return true;
            }
            return false;
        }

        public static bool IsRelationshipLevelsCorrect(string parentPrefix, string childPrefix)
        {
            if (parentPrefix == childPrefix)
                return false;
            if (! areaRepos.IsRelationshipLevelsCorrect(childPrefix, parentPrefix))
                return false;
            return true;
        }

    }
}
