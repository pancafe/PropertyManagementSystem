using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using System.Dynamic;
using System.IO;
using System.Text;
using PMS.Models;
using PMS.filter;

namespace PMS.Controllers
{
    [MyCheckFilterAttribute(IsCheck = true)]
    public class PropertyCompanyController : Controller
    {
        private PMS_EF db = new PMS_EF();

        public string type = "";
       

        // GET: /PropertyCompany/
        public ActionResult Index()
        {
            Session["loginname"]= Request.Params["displayname"];

            //计算用户数
            var num = db.owner.Count() + db.tenant.Count()+db.propertycompany.Count();
            ViewBag.num = num;

            //计算留言数
            var sug = db.owner.Where(u => u.Osuggestion != null).Count();
            var sugs = db.owner.Where(u => u.Osuggestion != null);
            ViewBag.sug = sug;

            List<dynamic> oneList = new List<dynamic>();
            foreach (var item in sugs.ToList())
            {
                dynamic dyObj = new ExpandoObject();
                dyObj.Oname = item.Oname;
                dyObj.Osuggestion = item.Osuggestion;
                oneList.Add(dyObj);
            }
            ViewBag.data = oneList;



            return View(db.propertycompany.ToList());
        }

        public ActionResult logout()
        {
            Session.Abandon();
            return Content("<script>window.location.href='/Home/Index';</script>");
           //return RedirectToAction("Index", "Home");
        }

        //新增住房向导
        [HttpPost]
        public ActionResult lyear_forms_elements(FormCollection fc)
        {
           
            
            if (fc["area"] == "" || fc["address"] == "" || fc["oid"] == "" || fc["oname"] == "" || fc["ocontact"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                //住房向导添加获取表单信息
                string area = fc["area"];
                string address = fc["address"];
                int oid = Convert.ToInt32(fc["oid"]);
                string oname = fc["oname"];
                string ocontact = fc["ocontact"];

                var has = db.house.Where(u=>u.Haddress==address);
                if (has.Count() > 0)
                {
                    return Content("<script>alert('该住宅已存在，请重新输入!');history.go(-1);</script>");
                }

                else
                {
                    var com = db.owner.Where(b => b.Oid == oid & b.Oname == oname);
                    if (com.Count() > 0)
                    {

                        var newhouse = new house()
                        {
                            Harea = area,
                            Haddress = address,
                            Oid = oid,
                            Oname = oname,
                            Ocontact = ocontact

                        };
                        db.house.Add(newhouse);
                        db.SaveChanges();
                        return Content("<script>alert('提交成功！');window.location.href='/PropertyCompany/lyear_pages_doc';</script>");

                    }
                    else
                    {
                        db.Dispose();
                        return Content("<script>alert('查无对应的业主编码或业主姓名');history.go(-1);</script>");
                    }
                }

               
            }


        }

        //住房信息维护_查询住房信息
        [HttpPost]
        public ActionResult Search(FormCollection fc)
        {
            string type = fc["type"];
            string keyword = fc["keyword"];

            
                if (keyword == "")
                {
                    return Content("<script>alert('请输入编码以进行查询');history.go(-1);</script>");
                }
                else
                {
                    int no = Convert.ToInt32(keyword);

                    if (type == "" || type == "houseid")//按住宅编码查询
                    {
                        var com = db.house.Where(b => b.Hid == no );
                        if (com.Count() > 0)
                        {
                            return View("lyear_pages_doc",com);
                        }
                        else
                        {
                            return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                        }
                       
                    }
                    else//按业主编码查询
                    {
                        var com = db.house.Where(b => b.Oid == no);
                        if (com.Count() > 0)
                        {
                            return View("lyear_pages_doc", com);
                        }
                        else
                        {
                            return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                        }
                    }


                }

        }

        
        //业主信息维护_查询业主信息
        [HttpPost]
        public ActionResult Search2(FormCollection fc)
        {

            string keyword = fc["keyword"];


            if (keyword == "")
            {
                return Content("<script>alert('请输入编码以进行查询');history.go(-1);</script>");
            }
            else
            {
                int no = Convert.ToInt32(keyword);

                var com = db.owner.Where(b => b.Oid == no);

                if (com.Count() > 0)
                {
                    return View("ownerinfo_search_del_edit", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }


            }
        }

        //费项信息维护_费项查询
        [HttpPost]
        public ActionResult Search3(FormCollection fc)
        {
           string keyword = fc["keyword"];


            if (keyword == "")
            {
                return Content("<script>alert('请输入编码以进行查询');history.go(-1);</script>");
            }
            else
            {
                int no = Convert.ToInt32(keyword);

                var com = db.feeitems.Where(b => b.Fid == no);

                if (com.Count() > 0)
                {
                    return View("feeitem_search_del_edit", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }


            }
        }

        //车位信息维护_车位查询
        [HttpPost]
        public ActionResult Search4(FormCollection fc)
        {
            string keyword = fc["keyword"];


            if (keyword == "")
            {
                return Content("<script>alert('请输入编码以进行查询');history.go(-1);</script>");
            }
            else
            {
                int no = Convert.ToInt32(keyword);

                var com = db.parkingspace.Where(b => b.PSid == no);

                if (com.Count() > 0)
                {
                    return View("parking_search_del_edit", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }


            }
        }

        //服务费项维护_费项查询
        [HttpPost]
        public ActionResult Search5(FormCollection fc)
        {
           string keyword = fc["keyword"];


            if (keyword == "")
            {
                return Content("<script>alert('请输入编码以进行查询');history.go(-1);</script>");
            }
            else
            {
                int no = Convert.ToInt32(keyword);

                var com = db.service.Where(b => b.Sid == no);

                if (com.Count() > 0)
                {
                    return View("service_search_del_edit", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }


            }
        }

        //业主申请服务_按日期查询
        [HttpPost]
        public ActionResult SearchByDate(FormCollection fc)
        {
            string d1 = fc["date1"];
            string d2 = fc["date2"];

            if (d1 == "" || d2 == "")
            {
                return Content("<script>alert('请输入日期后以进行查询');history.go(-1);</script>");
            }
            else
            {
                DateTime dt1 = Convert.ToDateTime(d1);
                DateTime dt2 = Convert.ToDateTime(d2);

                var com = db.application.Where(b => b.Atime >= dt1 && b.Atime <= dt2);
                if (com.Count() > 0)
                {
                    return View("applicaionfromowner", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }
            }

        }

        //按申请时间排序业主申请服务
        public ActionResult applicationorderbydate()
        {
            return View("applicaionfromowner",db.application.OrderBy(a=>a.Atime));
        }
        public ActionResult applicationorderbydatedes()
        {
            return View("applicaionfromowner",db.application.OrderByDescending(a=>a.Atime));
        }

        //编辑住房信息
        [HttpPost]
        public ActionResult houseinfoEdit(FormCollection fc)
        {

            if (fc["area"] == "" || fc["address"] == "" || fc["oid"] == "" || fc["oname"] == "" || fc["ocontact"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {
                string area = fc["area"];
                string address = fc["address"];
                int oid = Convert.ToInt32(fc["oid"]);
                string oname = fc["oname"];
                string ocontact = fc["ocontact"];

                var com = db.owner.Where(b => b.Oid == oid & b.Oname == oname);
                if (com.Count() > 0)
                {

                    var house=db.house.FirstOrDefault(u=>u.Haddress==address);
                    if (house != null)
                    {
                        house.Harea = area;
                        house.Oid = oid;
                        house.Oname = oname;
                        house.Ocontact = ocontact;

                    }
                    db.SaveChanges();
                    return Content("<script>alert('提交成功！');window.location.href='/PropertyCompany/lyear_pages_doc';</script>");

                }
                else
                {
                    db.Dispose();
                    return Content("<script>alert('查无对应的业主编码或业主姓名');history.go(-1);</script>");
                }
            }
           

        }

        //编辑业主信息
        [HttpPost]
        public ActionResult ownerinfoEdit(FormCollection fc)
        {
           if (fc["oname"] == "" || fc["osex"] == "0" || fc["ocontact"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
           else
           {
               string osex = fc["osex"];
               string oname = fc["oname"];
               string ocontact = fc["ocontact"];
               int oid = Convert.ToInt32(Session["Oid"]);

               if (osex == "1")
               {
                   osex = "男";
               }
               else if (osex == "2")
               {
                   osex = "女";
               }

               
                   var owner = db.owner.FirstOrDefault(u => u.Oid == oid);
                   if (owner != null)
                   {
                       owner.Osex = osex;
                       owner.Oname = oname;
                       owner.Ocontact = ocontact;
                       owner.Pid = 1;

                   }
                   db.SaveChanges();
                   return Content("<script>alert('修改成功！');window.location.href='/PropertyCompany/ownerinfo_search_del_edit';</script>");

              
           }
        }

        //编辑费项信息
        [HttpPost]
        public ActionResult feeiteminfoEdit(FormCollection fc)
        {
            if (fc["fname"] == "" || fc["fperiod"] == "0" || fc["fprice"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
           else
           {
               string fname = fc["fname"];
               string fperiod = fc["fperiod"];
               string fprice = fc["fprice"];
               int fid = Convert.ToInt32(Session["Fid"]);

               double fprice1 = Convert.ToDouble(fprice);

               if (fperiod == "1")
               {
                   fperiod = "月";
               }
               else if (fperiod == "2")
               {
                   fperiod = "季度";
               }
               else if (fperiod == "3")
               {
                   fperiod = "年";
               }

               var feeitems = db.feeitems.FirstOrDefault(u=>u.Fid==fid);

               if (feeitems != null)
                   {
                       feeitems.Fname = fname;
                       feeitems.Fperiod = fperiod;
                       feeitems.Fprice = fprice1;

                   }
                   db.SaveChanges();
                   return Content("<script>alert('修改成功！');window.location.href='/PropertyCompany/feeitem_search_del_edit';</script>");

              
           }
        }

        //编辑公告信息
        [HttpPost]
        public ActionResult noticeEdit(FormCollection fc)
        {
           if (fc["title"] == "" ||  fc["content"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
           else
           {
               string title = fc["title"];
               string content = fc["content"];
               int pid = Convert.ToInt32(Session["Pid"]);

               var propertycompany = db.propertycompany.FirstOrDefault(u => u.Pid == pid);


               if (propertycompany != null)
                   {
                       propertycompany.Pnotice = title;
                       propertycompany.Prules = content;

                   }
                   db.SaveChanges();
                   return Content("<script>alert('修改成功！');window.location.href='/PropertyCompany/notice_search_del_edit';</script>");

              
           }
        }


        //编辑服务信息
        [HttpPost]
        public ActionResult serviceinfoEdit(FormCollection fc)
        {
           if (fc["stype"] == "" || fc["sprice"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                //添加获取表单信息
                string stype = fc["stype"];
                string sprice = fc["sprice"];
                int sid = Convert.ToInt32(Session["Sid"]);

                double sprice1 = Convert.ToDouble(sprice);

                var service = db.service.FirstOrDefault(u=>u.Sid==sid);

               if (service != null)
                   {
                       service.Stype = stype;
                       service.Sprice = sprice1;
                   }
                   db.SaveChanges();
                return Content("<script>alert('修改成功！');window.location.href='/PropertyCompany/service_search_del_edit';</script>");

            }
        }

        //重设密码
        [HttpPost]
        public ActionResult passwordreset(FormCollection fc)
        {
           if (fc["oldpass"] == "" || fc["newpass"] == "" || fc["repass"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
           else if (fc["newpass"] !=  fc["repass"])
           {
               return Content("<script>alert('密码不一致！');history.go(-1);</script>");
            }
            else
            {

                //添加获取表单信息
               string old = fc["oldpass"];
               string newpass=fc["newpass"];
               string repass = fc["repass"];
                int pid = Convert.ToInt32(Session["Pid"]);

                var propertycompany = db.propertycompany.FirstOrDefault(u => u.Pid == pid && u.Ppass==old);
                

               if (propertycompany != null)
                   {
                       propertycompany.Ppass = newpass;
                   }
               else
               {
                   return Content("<script>alert('旧密码错误');history.go(-1);</script>");
               }
                   db.SaveChanges();
                   return Content("<script>alert('密码修改成功！');history.go(-1);</script>");

            }
        }

        //编辑车位信息
        [HttpPost]
        public ActionResult parkinginfoEdit(FormCollection fc)
        {
            

            if (fc["pslocation"] == "" || fc["pstype"] == "0" || fc["psprice"] == "" || fc["psstatus"] == "0" || fc["psarea"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {
                string pslocation = fc["pslocation"];
                string pstype = fc["pstype"];
                string psprice = fc["psprice"];
                string psstatus = fc["psstatus"];
                string psarea = fc["psarea"];
                int psid = Convert.ToInt32(Session["PSid"]);

                double psprice1 = Convert.ToDouble(psprice);

                if (fc["pstype"] == "1")
                {
                    pstype = "地上";
                }
                else if (fc["pstype"] == "2")
                {
                    pstype = "地下";
                }

                if (fc["psstatus"] == "1")
                {
                    psstatus = "空闲";
                }
                else if (fc["psstatus"] == "2")
                {
                    psstatus = "已售";
                }

                var parkingspace = db.parkingspace.FirstOrDefault(u => u.PSid == psid);

                if (parkingspace != null)
                {
                    parkingspace.PSlocation = pslocation;
                    parkingspace.PSarea = psarea;
                    parkingspace.PSprice = psprice1;
                    parkingspace.PSstatus = psstatus;
                    parkingspace.PStype = pstype;

                }
                db.SaveChanges();
                return Content("<script>alert('修改成功！');window.location.href='/PropertyCompany/parking_search_del_edit';</script>");


            }
        }

        //按照住房面积排序
        public ActionResult orderbyarea()
        {
            return View("lyear_pages_doc",db.house.OrderBy(h=>h.Harea));
        }
        public ActionResult orderbyareades()
        {
             return View("lyear_pages_doc",db.house.OrderByDescending(h=>h.Harea));
        }

        //删除住房信息
        public ActionResult housedelete(int? id)
        {
            house house = db.house.Find(id);

            db.house.Remove(house);
            db.SaveChanges();

            return Content("<script>alert('删除成功！');window.location.href='/PropertyCompany/lyear_pages_doc';</script>");
        }

        //删除业主信息
        public ActionResult ownerdelete(int? id)
        {
            owner owner = db.owner.Find(id);
            db.owner.Remove(owner);
            db.SaveChanges();
            return Content("<script>alert('删除成功！');window.location.href='/PropertyCompany/ownerinfo_search_del_edit';</script>");
        }

        //删除费项信息
        public ActionResult feeitemdelete(int? id)
        {
            feeitems feeitems = db.feeitems.Find(id);
            db.feeitems.Remove(feeitems);
            db.SaveChanges();
            return Content("<script>alert('删除成功！');window.location.href='/PropertyCompany/feeitem_search_del_edit';</script>");
        }

        //删除服务信息
        public ActionResult servicedelete(int? id)
        {
            service service = db.service.Find(id);
            db.service.Remove(service);
            db.SaveChanges();
            return Content("<script>alert('删除成功！');window.location.href='/PropertyCompany/service_search_del_edit';</script>");
        }

        //删除车位记录
        public ActionResult recorddelete(int? id)
        {
            //实际完成的逻辑操作是将表中的车位记录设为空值

            var tenant = db.tenant.FirstOrDefault(u => u.Tid == id);

            //获取车位号
            var psid = db.tenant.Where(u => u.Tid == id).Select(u => u.PSid).FirstOrDefault();

            if (tenant != null)
            {
                tenant.PSid = null;

            }

            var ps = db.parkingspace.FirstOrDefault(u=>u.PSid==psid);
            if (ps != null)
            {
                ps.PSstatus = "空闲";
            }

            db.SaveChanges();
            return Content("<script>alert('删除成功！');window.location.href='/PropertyCompany/parkingspacefromtenant';</script>");


        }

        //删除公告信息
        public ActionResult noticedelete(int? id)
        {
            //实际完成的逻辑操作是将表中的公告设为空值

            var propertycompany = db.propertycompany.FirstOrDefault(u => u.Pid == id);

            if (propertycompany != null)
            {
                propertycompany.Prules = null;
                propertycompany.Pnotice = null;

            }
            db.SaveChanges();
            return Content("<script>alert('删除成功！');window.location.href='/PropertyCompany/notice_search_del_edit';</script>");
        }

        //删除车位信息
        public ActionResult parkingdelete(int? id)
        {
            parkingspace parkingspace = db.parkingspace.Find(id);
            db.parkingspace.Remove(parkingspace);
            db.SaveChanges();
            return Content("<script>alert('删除成功！');window.location.href='/PropertyCompany/parking_search_del_edit';</script>");
        }

        //删除业主申请服务记录
        public ActionResult applicationdelete(int? id)
        {
            application application = db.application.Find(id);

            db.application.Remove(application);
            db.SaveChanges();

            return Content("<script>alert('删除成功！');window.location.href='/PropertyCompany/applicaionfromowner';</script>");
        }



        //新增业主
        [HttpPost]
        public ActionResult ownerAdd(FormCollection fc)
        {
            if (fc["oname"] == "" || fc["osex"] == "0" || fc["ocontact"] == "" || fc["oacct"] == "" || fc["opass"]=="")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                //添加获取表单信息
                string oname = fc["oname"];
                string ocontact = fc["ocontact"];
                string osex = fc["osex"];
                string oacct = fc["oacct"];
                string opass = fc["opass"];


                if (fc["osex"] == "1")
                {
                     osex = "男";
                }
                else if(fc["osex"]=="2")
                {
                     osex = "女";
                }

                    var newowner = new owner()
                    {
                        Osex=osex,
                        Oname = oname,
                        Ocontact = ocontact,
                        Oacct=oacct,
                        Opass=opass,
                        Pid=1

                    };
                    db.owner.Add(newowner);
                    db.SaveChanges();
                    return Content("<script>alert('添加成功！');window.location.href='/PropertyCompany/ownerinfo_search_del_edit';</script>");

            }
        }


        //新增费项
        [HttpPost]
        public ActionResult feeitemAdd(FormCollection fc)
        {
            if (fc["fname"] == "" || fc["fperiod"] == "0" || fc["fprice"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                //添加获取表单信息
                string fname = fc["fname"];
                string fprice = fc["fprice"];
                string fperiod = fc["fperiod"];

                double fprice1 = Convert.ToDouble(fprice);


                if (fc["fperiod"] == "1")
                {
                    fperiod = "月";
                }
                else if (fc["fperiod"] == "2")
                {
                    fperiod = "季度";
                }
                else if (fc["fperiod"] == "3")
                {
                    fperiod = "年";
                }

                var has = db.feeitems.Where(f=>f.Fname==fname);
                if (has.Count() > 0)
                {
                    return Content("<script>alert('该名称的费项已存在');history.go(-1);</script>");
                }
                else
                {
                    var newfeeitem = new feeitems()
                    {
                        Fname = fname,
                        Fperiod = fperiod,
                        Fprice = fprice1,
                        Pid = 1
                    };

                    db.feeitems.Add(newfeeitem);
                    db.SaveChanges();
                    return Content("<script>alert('添加成功！');window.location.href='/PropertyCompany/feeitem_search_del_edit';</script>");
                }

                

            }
        }

        //新增服务
        [HttpPost]
        public ActionResult serviceAdd(FormCollection fc)
        {
           if (fc["stype"] == "" || fc["sprice"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                //添加获取表单信息
                string stype = fc["stype"];
                string sprice = fc["sprice"];

                double sprice1 = Convert.ToDouble(sprice);

                var has = db.service.Where(s=>s.Stype==stype);
                if (has.Count() > 0)
                {
                    return Content("<script>alert('该服务已存在，请重新输入');history.go(-1);</script>");
                }
                else
                {
                    var newservice = new service
                    {
                        Stype = stype,
                        Sprice = sprice1,
                        Pid = 1
                    };

                    db.service.Add(newservice);

                    db.SaveChanges();
                    return Content("<script>alert('添加成功！');window.location.href='/PropertyCompany/service_search_del_edit';</script>");
                }


                

            }
        }

        //新增通知
        [HttpPost]
        public ActionResult notice(FormCollection fc)
        {
            if (fc["title"] == "" || fc["content"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                string title = fc["title"];
                string content = fc["content"];

                string pacct = Session["loginname"].ToString();
                var property = db.propertycompany.FirstOrDefault(u => u.Pacct == pacct);

               if (property != null)
                {
                   property.Pnotice = title;
                   property.Prules = content;
               }

                db.SaveChanges();
                return Content("<script>alert('添加成功！');window.location.href='/PropertyCompany/notice_search_del_edit';</script>");

            }
        }


        //新增车位
        [HttpPost]
        public ActionResult parkingAdd(FormCollection fc)
        {
           if (fc["pslocation"] == "" || fc["pstype"] == "0" || fc["psprice"] == "" || fc["psstatus"]=="0" || fc["psarea"]=="")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                //添加获取表单信息
                string pslocation = fc["pslocation"];
                string pstype = fc["pstype"];
                string psprice = fc["psprice"];
                string psstatus = fc["psstatus"];
                string psarea = fc["psarea"];

                double psprice1 = Convert.ToDouble(psprice);

                if (fc["pstype"] == "1")
                {
                    pstype = "地上";
                }
                else if (fc["pstype"] == "2")
                {
                    pstype = "地下";
                }

                if (fc["psstatus"] == "1")
                {
                    psstatus = "空闲";
                }
                else if (fc["psstatus"] == "2")
                {
                    psstatus = "已售";
                }

                var has = db.parkingspace.Where(p=>p.PSlocation==pslocation);
                if (has.Count() > 0)
                {
                    return Content("<script>alert('该车位已存在，请重新输入');history.go(-1);</script>");
                }
                else
                {
                    var newparking = new parkingspace()
                    {
                        PSarea = psarea,
                        PSlocation = pslocation,
                        PSprice = psprice1,
                        PSstatus = psstatus,
                        PStype = pstype,
                        Pid = 1
                    };

                    db.parkingspace.Add(newparking);
                    db.SaveChanges();
                    return Content("<script>alert('添加成功！');window.location.href='/PropertyCompany/parking_search_del_edit';</script>");
                }


              

            }
        }





        //业主申请服务受理状态更改
        public ActionResult applicationstatuschange(int? id)
        {
                   var application = db.application.FirstOrDefault(u => u.Aid == id &  u.Astatus=="未受理");
                   if (application != null)
                   {
                       application.Astatus = "已受理";
                       db.SaveChanges();
                       return Content("<script>alert('受理成功！');window.location.href='/PropertyCompany/applicaionfromowner';</script>");

                   }
                   else
                   {
                       return Content("<script>alert('该项已受理，不需要重复点击');window.location.href='/PropertyCompany/applicaionfromowner';</script>");
                   }
                  
        }

        //组合查询
        [HttpPost]
        public ActionResult CombineSearch(FormCollection fc)
        {
            if (fc["pay"] == "" & fc["date1"] == "" & fc["date2"] == "" & fc["keyword"] == "")
            {
                return Content("<script>alert('请至少选择一项进行查询');history.go(-1);</script>");
            }
            else
            {
                if (fc["pay"] != "" & fc["date1"] == "" & fc["date2"] == "" & fc["keyword"] == "")//已缴/未缴
                {
                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示未缴账单";

                        return View("bill_search_del_edit", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示已缴账单";
                       

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }
                }
                else if (fc["pay"] != "" & fc["date1"] != "" & fc["date2"] != "" & fc["keyword"] == "")//已缴未缴+日期
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                    DateTime dt1 = Convert.ToDateTime(d1);
                    DateTime dt2 = Convert.ToDateTime(d2);

                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0 && b.Btime >= dt1 && b.Btime <= dt2);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在"+d1+"-"+d2+"之间未缴的账单";

                        return View("bill_search_del_edit", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0 && b.Btime > dt1 && b.Btime < dt2);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间已缴的账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }

                }
                else if (fc["pay"] != "" & fc["date1"] == "" & fc["date2"] == "" & fc["keyword"] != "")//已缴未缴+租户编码
                {
                    string keyword = fc["keyword"];
                    int no = Convert.ToInt32(keyword);

                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0 && b.Tid == no);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示租户"  + no + "未缴的账单";

                        return View("bill_search_del_edit", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0 && b.Tid == no);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示租户" + no + "已缴的账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }

                }
                else if (fc["pay"] != "" & fc["date1"] != "" & fc["date2"] != "" & fc["keyword"] != "")//已缴未缴+日期+租户编码
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                    DateTime dt1 = Convert.ToDateTime(d1);
                    DateTime dt2 = Convert.ToDateTime(d2);

                    string keyword = fc["keyword"];
                    int no = Convert.ToInt32(keyword);

                    if (fc["pay"] == "未缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid != 0 && b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间租户"+no+"未缴的账单";

                        return View("bill_search_del_edit", com);
                    }
                    else if (fc["pay"] == "已缴")
                    {
                        var com = db.bill.Where(b => b.Bunpaid == 0 && b.Btime > dt1 && b.Btime < dt2 && b.Tid == no);
                        //计算总计，已收，未收
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2 && b.Tid == no).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2 && b.Tid == no).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime > dt1 && b.Btime < dt2 && b.Tid == no).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间租户" + no + "已缴的账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                    }

                }
                else if (fc["pay"] == "" & fc["date1"] != "" & fc["date2"] != "" & fc["keyword"] == "")//日期
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                        DateTime dt1 = Convert.ToDateTime(d1);
                        DateTime dt2 = Convert.ToDateTime(d2);

                        var com = db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2);
                        if (com.Count() > 0)
                        {
                            //计算该日期区间的缴纳情况
                            double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2).Sum(s => s.Bprice));
                            double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2).Sum(s => s.Bpaid));
                            double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2).Sum(s => s.Bunpaid));
                            ViewBag.sum = sum;
                            ViewBag.accepted = accepted;
                            ViewBag.unaccepted = unaccepted;

                            ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间的账单";

                            return View("bill_search_del_edit", com);
                        }
                        else
                        {
                            return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                        }
                    
                }
                else if (fc["pay"] == "" & fc["date1"] != "" & fc["date2"] != "" & fc["keyword"] != "")//日期+编码
                {
                    string d1 = fc["date1"];
                    string d2 = fc["date2"];

                    DateTime dt1 = Convert.ToDateTime(d1);
                    DateTime dt2 = Convert.ToDateTime(d2);

                    string keyword = fc["keyword"];
                    int no = Convert.ToInt32(keyword);

                    var com = db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no);
                    if (com.Count() > 0)
                    {
                        //计算该日期区间的缴纳情况
                        double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no).Sum(s => s.Bprice));
                        double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no).Sum(s => s.Bpaid));
                        double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2 && b.Tid == no).Sum(s => s.Bunpaid));
                        ViewBag.sum = sum;
                        ViewBag.accepted = accepted;
                        ViewBag.unaccepted = unaccepted;

                        ViewBag.pagestatus = "当前显示在" + d1 + "-" + d2 + "之间租户"+no+"的账单";

                        return View("bill_search_del_edit", com);
                    }
                    else
                    {
                        return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                    }

                }
                else if (fc["pay"] == "" & fc["date1"] == "" & fc["date2"] == "" & fc["keyword"] != "")//编码
                {
                    string keyword = fc["keyword"];

                        int no = Convert.ToInt32(keyword);

                        var com = db.bill.Where(b => b.Tid == no);

                        if (com.Count() > 0)
                        {
                            //计算该租户的缴纳情况
                            //计算总计，已收，未收
                            double sum = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bprice));
                            double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bpaid));
                            double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bunpaid));
                            ViewBag.sum = sum;
                            ViewBag.accepted = accepted;
                            ViewBag.unaccepted = unaccepted;

                            ViewBag.pagestatus = "当前显示租户" + no + "的账单";

                            return View("bill_search_del_edit", com);
                        }
                        else
                        {
                            return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                        }

                }
                else
                {
                    return Content("<script>alert('未知错误>_<');history.go(-1);</script>");
                }
            }

        }




        //查询账单记录_日期查询
        [HttpPost]
        public ActionResult SearchByDate1(FormCollection fc)
        {
            string d1 = fc["date1"];
            string d2 = fc["date2"];

            if (d1 == "" || d2 == "")
            {
                return Content("<script>alert('请输入日期后以进行查询');history.go(-1);</script>");
            }
            else
            {
                DateTime dt1 = Convert.ToDateTime(d1);
                DateTime dt2 = Convert.ToDateTime(d2);

                var com = db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2);
                if (com.Count() > 0)
                {
                    //计算该日期区间的缴纳情况
                    double sum = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2).Sum(s => s.Bprice));
                    double accepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2).Sum(s => s.Bpaid));
                    double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Btime >= dt1 && b.Btime <= dt2).Sum(s => s.Bunpaid));
                    ViewBag.sum = sum;
                    ViewBag.accepted = accepted;
                    ViewBag.unaccepted = unaccepted;


                    return View("bill_search_del_edit", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }
            }
        }

        //搜索记录_根据租户编码
        [HttpPost]
        public ActionResult Search11(FormCollection fc)
        {
            string keyword = fc["keyword"];


            if (keyword == "")
            {
                return Content("<script>alert('请输入编码以进行查询');history.go(-1);</script>");
            }
            else
            {
                int no = Convert.ToInt32(keyword);

                var com = db.bill.Where(b => b.Tid == no);

                if (com.Count() > 0)
                {
                    //计算该租户的缴纳情况
                    //计算总计，已收，未收
                    double sum = Convert.ToDouble(db.bill.Where(b=>b.Tid==no).Sum(s => s.Bprice));
                    double accepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bpaid));
                    double unaccepted = Convert.ToDouble(db.bill.Where(b => b.Tid == no).Sum(s => s.Bunpaid));
                    ViewBag.sum = sum;
                    ViewBag.accepted = accepted;
                    ViewBag.unaccepted = unaccepted;

                    return View("bill_search_del_edit", com);
                }
                else
                {
                    return Content("<script>alert('查无相关记录');history.go(-1);</script>");
                }


            }
        }

        //未缴已缴查看
        public ActionResult unpaidpage()
        {
           // string loginname = Session["loginname"].ToString();
           // var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b =>  b.Bunpaid != 0);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            return View("bill_search_del_edit", com);
        }
        public ActionResult paidpage()
        {
            //string loginname = Session["loginname"].ToString();
            //var oid = db.owner.Where(b => b.Oname == loginname).Select(b => b.Oid).FirstOrDefault();

            var com = db.bill.Where(b =>  b.Bunpaid == 0);

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;


            return View("bill_search_del_edit", com);
        }

        //删除上个月已缴纳的账单
        public ActionResult deletelastmonth()
        {
            //获取当前系统时间 设置为 年-月-01
            string time = DateTime.Now.Year.ToString() +"-"+ DateTime.Now.Month.ToString()+"-01";
            DateTime dt = Convert.ToDateTime(time);
            //筛选在此日期之前并且已缴纳的账单
            db.bill.Where(d => d.Btime < dt && d.Bunpaid == 0).ToList().ForEach(d=>db.bill.Remove(d));
            db.SaveChanges();
           

            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            return Content("<script>alert('已删除" + time + "之前已缴的账单');window.location.href='/PropertyCompany/bill_search_del_edit';</script>");
        }



        //新增租户账单
        [HttpPost]
        public ActionResult billAdd(FormCollection fc)
        {

            if (fc["tid"] == "" || fc["tname"] == "" || fc["usage"] == "" || fc["price"] == "" || fc["oid"] == "")
            {
                return Content("<script>alert('请填写全部信息');history.go(-1);</script>");
            }
            else
            {

                int tid = Convert.ToInt32(fc["tid"]);
                string tname = fc["tname"];
                int oid = Convert.ToInt32(fc["oid"]);
                string fee = fc["fee"];
                string usage = fc["usage"];
                string price = fc["price"];
                DateTime time = DateTime.Now;

                double usage1 = Convert.ToDouble(usage);
                double price1 = Convert.ToDouble(price);

                ViewBag.pagestatus = "当前显示全部账单";

                var newbill = new bill
                {
                    Tid = tid,
                    Tname = tname,
                    Fname = fee,
                    Busage = usage1,
                    Bprice = price1,
                    Btime = time,
                    Oid = oid,
                    Bpaid = 0,
                    Bunpaid = price1
                };


                db.bill.Add(newbill);
                db.SaveChanges();
                return Content("<script>alert('添加成功！');window.location.href='/PropertyCompany/bill_search_del_edit';</script>");

            }
        }

        //删除账单记录
        public ActionResult billdelete(int? id, string name)
        {
            var bill = db.bill.FirstOrDefault(u => u.Tid == id && u.Fname == name);
            db.bill.Remove(bill);
            db.SaveChanges();
            ViewBag.pagestatus = "当前显示全部账单";
            return Content("<script>alert('删除成功！');window.location.href='/PropertyCompany/bill_search_del_edit';</script>");
        }

        //催缴短信发送
        public ActionResult billnotice(int? id, string name)
        {
            //获取目标电话号码smsMob
            var tel = db.tenant.Where(b => b.Tid == id).Select(b => b.Tcontact).FirstOrDefault();
            string smsMob = Convert.ToString(tel);

            string THE_UID = "weirouto";//网建用户名
            string THE_KEY = "d41d8cd98f00b204e980";//接口密钥


            var com = db.bill.Where(t => t.Tid == id && t.Bunpaid != 0);
            string smsText = "尊敬的阳光小区租户，您本月未缴项目如下，请及时缴纳以防止对应服务终止造成不便:";//短信内容

            foreach (var item in com.ToList())
            {
                smsText = smsText + "[" + item.Fname + ":" + item.Bunpaid + "]";
            }


            string postUrl = "http://utf8.sms.webchinese.cn/?Uid=" + THE_UID + "&key=" + THE_KEY + "&smsMob=" + smsMob + "&smsText=" + smsText;

            //发送短信
            try
            {
                HttpWebRequest hr = (HttpWebRequest)WebRequest.Create(postUrl);
                hr.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1)";
                hr.Method = "GET";
                hr.Timeout = 30 * 60 * 1000;
                WebResponse hs = hr.GetResponse();
                Stream sr = hs.GetResponseStream();
                StreamReader ser = new StreamReader(sr, Encoding.Default);

            }
            catch (Exception ex)
            {

                return Content("<script>alert('出现未知的错误>_<，如下:'" + ex.Message + ");history.go(-1);</script>");

            }

            return Content("<script>alert('通知（包含该租户全部未缴项目，不必重复发送）已发送!');history.go(-1);</script>");


        }

        //按照用量排序
        public ActionResult billorderbyusage()
        {
            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            return View("bill_search_del_edit",db.bill.OrderBy(b=>b.Busage));
        }
        public ActionResult billorderbyusagedes()
        {
             //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            return View("bill_search_del_edit",db.bill.OrderByDescending(b=>b.Busage));
        }

        //根据价格排序
        public ActionResult billorderbyprice()
        {
            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            return View("bill_search_del_edit",db.bill.OrderBy(b=>b.Bprice));
        }
        public ActionResult billorderbypricedes()
        {
             //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            return View("bill_search_del_edit",db.bill.OrderByDescending(b=>b.Bprice));
        }

        //根据日期排序
        public ActionResult billorderbydate()
        {
            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            return View("bill_search_del_edit",db.bill.OrderBy(b=>b.Btime));
        }
        public ActionResult billorderbydatedes()
        {
            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s => s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示全部账单";

            return View("bill_search_del_edit",db.bill.OrderByDescending(b=>b.Btime));
        }




        //侧边栏页面跳转
        public ActionResult billAdd()
        {

            var com = db.tenant;


            List<dynamic> tidList = new List<dynamic>();
            foreach (var item in com.ToList())
            {
                dynamic dyObj = new ExpandoObject();
                dyObj.Tid = item.Tid;
                dyObj.Tname = item.Tname;
                dyObj.Oid = item.Oid;
                tidList.Add(dyObj);
            }
            ViewBag.tids = tidList;

            return View(db.feeitems);
        }


        public ActionResult bill_search_del_edit()
        {
             
            //计算总计，已收，未收
            double sum = Convert.ToDouble(db.bill.Sum(s=>s.Bprice));
            double accepted = Convert.ToDouble(db.bill.Sum(s => s.Bpaid));
            double unaccepted = Convert.ToDouble(db.bill.Sum(s => s.Bunpaid));
            ViewBag.sum = sum;
            ViewBag.accepted = accepted;
            ViewBag.unaccepted = unaccepted;

            ViewBag.pagestatus = "当前显示所有租户账单";

            return View(db.bill.OrderBy(b=>b.Tid));
        }
        public ActionResult lyear_forms_elements()
        {
            return View();
        }
        public ActionResult lyear_pages_doc()
        {
            
            return View(db.house);
        }

        public ActionResult ownerAdd()
        {
            return View();
        }
        public ActionResult ownerinfo_search_del_edit()
        {
            return View(db.owner);
        }
        public ActionResult applicaionfromowner()
        {
            return View(db.application.OrderBy(a=>a.Oid));
        }
        public ActionResult feeitem_search_del_edit()
        {
            return View(db.feeitems);
        }
        public ActionResult parkingAdd()
        {
            return View();
        }
        public ActionResult parking_search_del_edit()
        {
            return View(db.parkingspace);
        }
        public ActionResult parkingspacefromtenant()
        {
            var query = from v1 in db.tenant
                        join v2 in db.parkingspace on v1.PSid equals v2.PSid
                        select new
                        {
                            v1.Tid,
                            v1.Tname,
                            v2.PSid,
                            v2.PStype,
                            v2.PSlocation
                        };

            List<dynamic> oneList=new List<dynamic>();
            foreach (var item in query.ToList())
            {
                dynamic dyObj = new ExpandoObject();
                dyObj.Tid = item.Tid;
                dyObj.Tname = item.Tname;
                dyObj.PSid = item.PSid;
                dyObj.PStype = item.PStype;
                dyObj.PSlocation = item.PSlocation;
                oneList.Add(dyObj);
            }
            ViewBag.data = oneList;
            //parkingspacefromtenant
            return View();
        }
        public ActionResult service_search_del_edit()
        {
            return View(db.service);
        }
        public ActionResult notice()
        {
            return View();
        }
        public ActionResult notice_search_del_edit()
        {
            return View(db.propertycompany);
        }
        public ActionResult passwordreset()
        {
            return View();
        }
        public ActionResult feeitemAdd()
        {
            return View();
        }
        public ActionResult serviceAdd()
        {
            return View();
        }

        public ActionResult feeiteminfoEdit(int? id)
        {
            feeitems feeitems = db.feeitems.Find(id);

            Session["Fid"] = id;

            return View("feeiteminfoEdit", feeitems);
        }
        public ActionResult houseinfoEdit(int? id)
        {
            house house = db.house.Find(id);

            return View("houseinfoEdit",house);
        }
        public ActionResult noticeEdit(int? id)
        {
            propertycompany propertycompany = db.propertycompany.Find(id);

            Session["Pid"] = id;

            return View("noticeEdit", propertycompany);
        }
        
        public ActionResult ownerinfoEdit(int? id)
        {
            owner owner = db.owner.Find(id);

            Session["Oid"] = id;

            return View("ownerinfoEdit",owner);
        }
        public ActionResult parkinginfoEdit(int? id)
        {
            parkingspace parkingspace = db.parkingspace.Find(id);

            Session["PSid"] = id;

            return View("parkinginfoEdit",parkingspace);
        }
        public ActionResult serviceinfoEdit(int? id)
        {
            service service = db.service.Find(id);

            Session["Sid"] = id;

            return View("serviceinfoEdit", service);
        }



        // GET: /PropertyCompany/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            propertycompany propertycompany = db.propertycompany.Find(id);
            if (propertycompany == null)
            {
                return HttpNotFound();
            }
            return View(propertycompany);
        }

        // GET: /PropertyCompany/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: /PropertyCompany/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include="Pid,Pacct,Ppass,Pnotice,Prules")] propertycompany propertycompany)
        {
            if (ModelState.IsValid)
            {
                db.propertycompany.Add(propertycompany);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(propertycompany);
        }

        // GET: /PropertyCompany/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            propertycompany propertycompany = db.propertycompany.Find(id);
            if (propertycompany == null)
            {
                return HttpNotFound();
            }
            return View(propertycompany);
        }

        // POST: /PropertyCompany/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include="Pid,Pacct,Ppass,Pnotice,Prules")] propertycompany propertycompany)
        {
            if (ModelState.IsValid)
            {
                db.Entry(propertycompany).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(propertycompany);
        }

        // GET: /PropertyCompany/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            propertycompany propertycompany = db.propertycompany.Find(id);
            if (propertycompany == null)
            {
                return HttpNotFound();
            }
            return View(propertycompany);
        }

        // POST: /PropertyCompany/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            propertycompany propertycompany = db.propertycompany.Find(id);
            db.propertycompany.Remove(propertycompany);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
