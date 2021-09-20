﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Senparc.CO2NET.Extensions;
using Senparc.Weixin.Helpers;
using Senparc.Weixin.TenPayV3.Apis;
using Senparc.Weixin.TenPayV3.Apis.BasePay;
using Senparc.Weixin.TenPayV3.Apis.Marketing;
using Senparc.Weixin.TenPayV3.Entities;
using Senparc.Weixin.TenPayV3.Helpers;
using Senparc.Weixin.TenPayV3.Test;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml.Linq;

namespace Senparc.Weixin.TenPayV3.Apis.Tests
{
    [TestClass()]
    public class MarketingApisTests : BaseTenPayTest
    {
        string openId = "olPjZjsXuQPJoV0HlruZkNzKc91E";//换成测试人的 OpenId

        #region 代金券接口

        CreateStockReturnJson createStockResult = null;
        DistributeStockReturnJson distributeStockResult = null;

        /// <summary>
        /// 创建代金券接口批次测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_1.shtml
        /// </summary>
        [TestMethod()]
        public void CreateStockAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // var sp_billno = 

            // 代金券批次发放使用规则 发放10张优惠券 预算1000分 每个用户限领10张 开启防刷
            var stock_use_rule = new CreateStockRequsetData.Stock_Use_Rule(10, 1000, null, 10, false, true);

            // 代金券使用规则 指明可使用本批次代金券的商户号
            var coupon_use_rule = new CreateStockRequsetData.Coupon_Use_Rule(null, null, null, null, null, null, null, new string[] { TenPayV3Info.MchId });

            // TODO:流水号?这样是否有效?
            var out_request_no = string.Format("{0}{1}{2}", TenPayV3Info.MchId/*10位*/, SystemTime.Now.ToString("yyyyMMddHHmmss"), TenPayV3Util.BuildRandomStr(6));

            var requestData = new CreateStockRequsetData("单元测试代金券批次", "用于单元测试", TenPayV3Info.MchId, new TenpayDateTime(DateTime.Now), new TenpayDateTime(DateTime.Now.AddDays(1)), stock_use_rule, null, coupon_use_rule, true, "NORMAL", out_request_no);

            var marketingApis = new MarketingApis();
            createStockResult = marketingApis.CreateStockAsync(requestData).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 创建代金券接口批次结果：" + createStockResult.ToJson(true));

            Assert.IsNotNull(createStockResult);
            Assert.IsTrue(createStockResult.ResultCode.Success);
            Assert.IsTrue(createStockResult.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 激活代金券批次接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_3.shtml
        /// </summary>
        [TestMethod()]
        public void StartStockAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 如果还未创建代金券批次 则创建新的代金券批次
            if (createStockResult is null)
            {
                CreateStockAsyncTest();
            }

            var requestData = new StartStockRequsetData(TenPayV3Info.MchId);

            var marketingApis = new MarketingApis();
            var result = marketingApis.StartStockAsync(createStockResult.stock_id, requestData).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 激活代金券批次接口结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 发放代金券批次接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_2.shtml
        /// </summary>
        [TestMethod()]
        public void DistributeStockAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 如果还未创建代金券批次 则创建新的代金券批次 并且激活
            if (createStockResult is null)
            {
                StartStockAsyncTest();
            }

            var out_request_no = string.Format("{0}{1}{2}", TenPayV3Info.MchId/*10位*/, SystemTime.Now.ToString("yyyyMMddHHmmss"), TenPayV3Util.BuildRandomStr(6));

            var requestData = new DistributeStockRequsetData(createStockResult.stock_id, out_request_no, TenPayV3Info.AppId, TenPayV3Info.MchId);

            var marketingApis = new MarketingApis();
            distributeStockResult = marketingApis.DistributeStockAsync(openId, requestData).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 发放代金券批次接口结果：" + distributeStockResult.ToJson(true));

            Assert.IsNotNull(distributeStockResult);
            Assert.IsTrue(distributeStockResult.ResultCode.Success);
            Assert.IsTrue(distributeStockResult.VerifySignSuccess == true);//通过验证
        }


        /// <summary>
        /// 暂停发放代金券批次接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_13.shtml
        /// </summary>
        [TestMethod()]
        public void PauseStockAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 如果还未创建代金券批次 则创建新的代金券批次 并且激活
            if (createStockResult is null)
            {
                StartStockAsyncTest();
            }

            var marketingApis = new MarketingApis();
            var result = marketingApis.PauseStockAsync(createStockResult.stock_id, TenPayV3Info.MchId).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 暂停发放代金券批次接口结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 重启发放代金券批次接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_14.shtml
        /// </summary>
        [TestMethod()]
        public void RestartStockAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 如果还未创建代金券批次 则创建新的代金券批次 激活 然后暂停发放
            if (createStockResult is null)
            {
                PauseStockAsyncTest();
            }

            var marketingApis = new MarketingApis();
            var result = marketingApis.RestartStockAsync(createStockResult.stock_id, TenPayV3Info.MchId).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 重启发放代金券批次接口结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 条件查询代金券批次接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_4.shtml
        /// </summary>
        [TestMethod()]
        public void QueryStocksAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            var marketingApis = new MarketingApis();
            var result = marketingApis.QueryStocksAsync(0, 10, TenPayV3Info.MchId).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 条件查询代金券批次接口结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 查询代金券批次详情接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_5.shtml
        /// </summary>
        [TestMethod()]
        public void QueryStockAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 如果还未创建代金券批次 则创建新的代金券批次
            if (createStockResult is null)
            {
                CreateStockAsyncTest();
            }

            var marketingApis = new MarketingApis();
            var result = marketingApis.QueryStockAsync(createStockResult.stock_id, TenPayV3Info.MchId).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 查询代金券批次详情接口结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 查询代金券详情接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_6.shtml
        /// </summary>
        [TestMethod()]
        public void QueryCouponAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 如果还未发放代金券 则发放代金券
            if (distributeStockResult is null)
            {
                DistributeStockAsyncTest();
            }

            var marketingApis = new MarketingApis();
            var result = marketingApis.QueryCouponAsync(distributeStockResult.coupon_id, TenPayV3Info.AppId, openId).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 查询代金券详情接口结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 查询代金券可用商户接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_7.shtml
        /// </summary>
        [TestMethod()]
        public void QueryMerchantsStockAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 如果还未发放代金券 则创建放代金券
            if (createStockResult is null)
            {
                CreateStockAsyncTest();
            }

            var marketingApis = new MarketingApis();
            var result = marketingApis.QueryMerchantsStockAsync(0, 50, TenPayV3Info.MchId, createStockResult.stock_id).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 查询代金券可用商户接口结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 查询代金券可用单品接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_8.shtml
        /// </summary>
        [TestMethod()]
        public void QueryItemsAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 如果还未发放代金券 则创建代金券
            if (createStockResult is null)
            {
                CreateStockAsyncTest();
            }

            var marketingApis = new MarketingApis();
            var result = marketingApis.QueryItemsAsync(0, 50, TenPayV3Info.MchId, createStockResult.stock_id).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 查询代金券可用单品接口结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 根据商户号查用户的券接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_9.shtml
        /// </summary>
        [TestMethod()]
        public void QueryCouponsAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 如果还未发放代金券 则创建代金券
            if (createStockResult is null)
            {
                CreateStockAsyncTest();
            }

            var marketingApis = new MarketingApis();
            var result = marketingApis.QueryCouponsAsync(openId, TenPayV3Info.AppId, createStockResult.stock_id, null, TenPayV3Info.MchId, null, null).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 下载批次核销明细结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 下载批次核销明细接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_10.shtml
        /// </summary>
        [TestMethod()]
        public void DownloadStockUseFlowAsyncTest()
        {
            // 如果还未发放代金券 则创建代金券
            if (createStockResult is null)
            {
                CreateStockAsyncTest();
            }

            var stock_id = createStockResult.stock_id;

            var filePath = $"{stock_id}_{SystemTime.Now.ToString("HHmmss")}-StockUseFlow.csv";
            Console.WriteLine("FilePath:" + filePath);
            var fs = new FileStream(filePath, FileMode.OpenOrCreate);
            var marketingApis = new MarketingApis();

            var result = marketingApis.DownloadStockUseFlowAsync(stock_id, fs).GetAwaiter().GetResult();
            fs.Flush();

            Console.WriteLine("FileStream Length:" + fs.Length);
            fs.Close();


            Console.WriteLine("微信支付 V3 下载批次退款明细结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
            Assert.IsTrue(File.Exists(filePath));

            ////校验文件
            //var fileHash = CO2NET.Helpers.FileHelper.GetFileHash(filePath, result.hash_type);
            //Assert.AreEqual(result.hash_value.ToUpper(), fileHash);
        }

        /// <summary>
        /// 下载批次核销明细接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_11.shtml
        /// </summary>
        [TestMethod()]
        public void DownloadStockRefundFlowAsyncTest()
        {
            // 如果还未发放代金券 则创建代金券
            if (createStockResult is null)
            {
                CreateStockAsyncTest();
            }

            var stock_id = createStockResult.stock_id;

            var filePath = $"{stock_id}_{SystemTime.Now.ToString("HHmmss")}-StockRefundFlow.csv";
            Console.WriteLine("FilePath:" + filePath);
            var fs = new FileStream(filePath, FileMode.OpenOrCreate);
            var marketingApis = new MarketingApis();

            var result = marketingApis.DownloadStockRefundFlowAsync(stock_id, fs).GetAwaiter().GetResult();
            fs.Flush();

            Console.WriteLine("FileStream Length:" + fs.Length);
            fs.Close();


            Console.WriteLine("微信支付 V3 交易账单查询结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
            Assert.IsTrue(File.Exists(filePath));

            ////校验文件
            //var fileHash = CO2NET.Helpers.FileHelper.GetFileHash(filePath, result.hash_type);
            //Assert.AreEqual(result.hash_value.ToUpper(), fileHash);
        }

        /// <summary>
        /// 设置营销事件消息通知地址接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_1_12.shtml
        /// </summary>
        [TestMethod()]
        public void SetNotifyUrlAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            var marketingApis = new MarketingApis();

            var requestData = new SetNotifyUrlRequsetData(TenPayV3Info.MchId, "https://unittesturl", null);

            var result = marketingApis.SetNotifyUrlAsync(requestData).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 设置营销事件消息通知地址结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }
        #endregion

        #region 商家券接口

        CreateBusifavorStockReturnJson createBusifavorStockResult = null;
        DistributeStockReturnJson distributeBusifavorStockResult = null;

        /// <summary>
        /// 创建商家券接口批次测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_2_1.shtml
        /// </summary>
        [TestMethod()]
        public void CreateBusifavorStockRequestDataAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // 商家券使用规则 即刻开始 发1小时 生效1天 满100减10
            var coupon_use_rule = new CreateBusifavorStockRequestData.Coupon_Use_Rule(new(new TenpayDateTime(DateTime.Now), new TenpayDateTime(DateTime.Now.AddHours(1)), 1, null, null, null), new(100, 10), null, null, "OFF_LINE", null, null);
            // 商家券发放规则
            var stock_send_rule = new CreateBusifavorStockRequestData.Stock_Send_Rule(10, 1, 10, true);

            // TODO:流水号?这样是否有效?
            var out_request_no = string.Format("{0}{1}{2}", TenPayV3Info.MchId/*10位*/, SystemTime.Now.ToString("yyyyMMddHHmmss"), TenPayV3Util.BuildRandomStr(6));

            var requestData = new CreateBusifavorStockRequestData("Senparc微信支付V3商家券测试", TenPayV3Info.MchId, null, "微信支付V3商家券测试使用", "NORMAL", coupon_use_rule, stock_send_rule, out_request_no, null, null, "WECHATPAY_MODE", null, false);

            var marketingApis = new MarketingApis();
            createBusifavorStockResult = marketingApis.CreateBusifavorStockRequestDataAsync(requestData).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 创建商家券接口批次结果：" + createBusifavorStockResult.ToJson(true));

            Assert.IsNotNull(createBusifavorStockResult);
            Assert.IsTrue(createBusifavorStockResult.ResultCode.Success);
            Assert.IsTrue(createBusifavorStockResult.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 查询商家券批次详情接口
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_2_2.shtml
        /// </summary>
        [TestMethod()]
        public void QueryBusifavorStockAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            var marketingApis = new MarketingApis();
            var result = marketingApis.QueryBusifavorStockAsync(createBusifavorStockResult.stock_id).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 查询商家券批次详情接口：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 核销商家券接口测试
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_2_3.shtml
        /// </summary>
        [TestMethod()]
        public void UseBusifavorCouponAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            // TODO:流水号?这样是否有效?
            var use_request_no = string.Format("{0}{1}{2}", TenPayV3Info.MchId/*10位*/, SystemTime.Now.ToString("yyyyMMddHHmmss"), TenPayV3Util.BuildRandomStr(6));

            var coupon_code = "coupon_code";// TODO: 发券似乎还是V2接口 或者 微信支付平台流量场景发放 https://pay.weixin.qq.com/wiki/doc/apiv3/open/pay/chapter5_2_1.shtml

            var requestData = new UseBusifavorCouponRequestData(coupon_code, createBusifavorStockResult.stock_id, TenPayV3Info.AppId, new TenpayDateTime(DateTime.Now), use_request_no, openId);

            var marketingApis = new MarketingApis();
            var result = marketingApis.UseBusifavorCouponAsync(requestData).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 核销商家券接口批次结果：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 根据过滤条件查询商家券用户券接口
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_2_4.shtml
        /// </summary>
        [TestMethod()]
        public void QueryBusifavorCouponsAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);
            var TenPayV3Info = TenPayV3InfoCollection.Data[key];

            var marketingApis = new MarketingApis();
            var result = marketingApis.QueryBusifavorCouponsAsync(openId, TenPayV3Info.AppId, createBusifavorStockResult.stock_id, null, null, null, null).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 根据过滤条件查询商家券用户券接口：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        /// <summary>
        /// 查询用户单张券详情接口
        /// https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter9_2_4.shtml
        /// </summary>
        [TestMethod()]
        public void QueryBusifavorCouponAsyncTest()
        {
            var key = TenPayHelper.GetRegisterKey(Config.SenparcWeixinSetting);

            var TenPayV3Info = TenPayV3InfoCollection.Data[key];
            var coupon_code = "coupon_code";// TODO: 发券似乎还是V2接口 或者 微信支付平台流量场景发放 https://pay.weixin.qq.com/wiki/doc/apiv3/open/pay/chapter5_2_1.shtml

            var marketingApis = new MarketingApis();
            var result = marketingApis.QueryBusifavorCouponAsync(coupon_code, TenPayV3Info.AppId, openId).GetAwaiter().GetResult();

            Console.WriteLine("微信支付 V3 查询用户单张券详情接口：" + result.ToJson(true));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.ResultCode.Success);
            Assert.IsTrue(result.VerifySignSuccess == true);//通过验证
        }

        #endregion

        #region 委托营销接口

        [TestMethod()]
        public void QueryPartnerships()
        {
            TerminatePartnershipsRequestData data = new(new("type", "appid", "mchid"), new("bussiness_type", "stock_id"));
            var test_result = data.ToJson();
            Console.WriteLine(data.ToJson());
        }

        #endregion
    }
}