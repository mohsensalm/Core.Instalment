using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace Core.DLL.Instalment
{
    [ApiController]
    [Route("Api/V1/Instalment/[Action]")]

    public class InstalmentController : ControllerBase
    {
        Instalment _Instalment = new Instalment();
        [HttpPost()]
        [ProducesResponseType(typeof(InstalmentResponseModel), 200)]
        public ActionResult GetFacilityInfo([FromBody] InstalmentRequestModel model)
        {
            try
            {
                return Ok(_Instalment.GetFacilityInfo(model));

            }
            catch (Exception ex)
            {

                return NotFound(ex.Message);
            }
        }

        [HttpPost()]
        [ProducesResponseType(typeof(InstalmentResponseModel), 200)]
        public ActionResult GetFacilityInfoWithRounding([FromBody] InstalmentRequestModel model)
        {
            try
            {
                return Ok(_Instalment.GetFacilityInfoWithRounding(model));

            }
            catch (Exception ex)
            {

                return NotFound(ex.Message);
            }
        }



    }
    public class Instalment
    { 
        private readonly PersianCalendar _pc;
        public Instalment()
        {
            _pc = new PersianCalendar();

        }
        public InstalmentResponseModel GetFacilityInfo(InstalmentRequestModel model)
        {
            var FacilityAmount = CalculateFacilityAmount(model.WholeAmount, model.ProfitRate, model.CountOfLoan, model.DurationOfLoanPayment);

           string PayDate=$"{_pc.GetYear(DateTime.Now)}/{_pc.GetMonth(DateTime.Now)}/{_pc.GetDayOfMonth(DateTime.Now)}";


            List<FacilityModel> Facility = new List<FacilityModel>();
            double PaymentedUntilNow = 0;

            for (byte Counter = 1; Counter <= model.CountOfLoan; Counter++)
            {


                if (Counter == 1)
                {
                    PaymentedUntilNow = 0;
                }
                else
                {
                    PaymentedUntilNow = PaymentedUntilNow + FacilityAmount;
                }
                var ShareAmount = CalculateLoanShare(model.WholeAmount, model.ProfitRate, model.DurationOfLoanPayment, PaymentedUntilNow);
                double RemainAmount;
                int Year=(Counter*model.DurationOfLoanPayment)/12;
               
                PayDate = $"{_pc.GetYear(DateTime.Now.AddYears(Year))}/{_pc.GetMonth(DateTime.Now.AddMonths((Counter-1)*model.DurationOfLoanPayment))}/{_pc.GetDayOfMonth(DateTime.Now)}";
                if (Counter == model.CountOfLoan)
                {
                    RemainAmount = 0;
                    Facility.Add(new FacilityModel { Id = Counter, Amount = (int)FacilityAmount, OrginalAmount = (FacilityAmount - ShareAmount), RemainAmount = RemainAmount, RemainOrginalAmount = RemainAmount, ShareAmount = ShareAmount, FacilityDate = PayDate });


                }
                else
                {
                    RemainAmount = CalculateRemainLoan(model.CountOfLoan, FacilityAmount, Counter) + ShareAmount;
                    Facility.Add(new FacilityModel { Id = Counter, Amount = (int)FacilityAmount, OrginalAmount = (FacilityAmount - ShareAmount), RemainAmount = RemainAmount, RemainOrginalAmount = RemainAmount - ShareAmount, ShareAmount = ShareAmount, FacilityDate = PayDate });

                }



            }

            InstalmentResponseModel Response = new InstalmentResponseModel();
            Response.FacilityList = Facility;
            Response.FacilityAmount = FacilityAmount;


            return Response;


        }
        public InstalmentResponseModel GetFacilityInfoWithRounding(InstalmentRequestModel model)

        {

            var FacilityAmount = RoundingNumber(CalculateFacilityAmount(model.WholeAmount, model.ProfitRate, model.CountOfLoan, model.DurationOfLoanPayment));

            string PayDate = $"{_pc.GetYear(DateTime.Now)}/{_pc.GetMonth(DateTime.Now)}/{_pc.GetDayOfMonth(DateTime.Now)}";



            List<FacilityModel> Facility = new List<FacilityModel>();
            double PaymentedUntilNow = 0;
            for (byte Counter = 1; Counter <= model.CountOfLoan; Counter++)
            {


                if (Counter == 1)
                {
                    PaymentedUntilNow = 0;
                }
                else
                {
                    PaymentedUntilNow = PaymentedUntilNow + FacilityAmount;
                }
                var ShareAmount = RoundingNumber(CalculateLoanShare(model.WholeAmount, model.ProfitRate, model.DurationOfLoanPayment, PaymentedUntilNow));
                double RemainAmount;
                int Year = (Counter * model.DurationOfLoanPayment) / 12;

                PayDate = $"{_pc.GetYear(DateTime.Now.AddYears(Year))}/{_pc.GetMonth(DateTime.Now.AddMonths((Counter - 1) * model.DurationOfLoanPayment))}/{_pc.GetDayOfMonth(DateTime.Now)}";

                if (Counter == model.CountOfLoan)
                {
                    RemainAmount = 0;

                    Facility.Add(new FacilityModel { Id = Counter, Amount = (int)FacilityAmount, OrginalAmount = FacilityAmount - ShareAmount, RemainAmount = RemainAmount, RemainOrginalAmount = RemainAmount, ShareAmount = ShareAmount, FacilityDate = PayDate });

                }
                else
                {
                    RemainAmount = RoundingNumber(CalculateRemainLoan(model.CountOfLoan, FacilityAmount, Counter)) + ShareAmount;
                    Facility.Add(new FacilityModel { Id = Counter, Amount = (int)FacilityAmount, OrginalAmount = FacilityAmount - ShareAmount, RemainAmount = RemainAmount, RemainOrginalAmount = RemainAmount - ShareAmount, ShareAmount = ShareAmount, FacilityDate = PayDate });
                }
                //FAcilityPayTime = FAcilityPayTime.AddMonths(model.DurationOfLoanPayment);

            }

            InstalmentResponseModel Response = new InstalmentResponseModel();
            Response.FacilityList = Facility;
            Response.FacilityAmount = FacilityAmount;




            return Response;


        }


        public double RoundingNumber(double number)
        {
            return Math.Ceiling((number / 100) * 100);
        }
        public double CalculateLoanShare(double wholeAmount, int profitRate, int durationOfLoanPaymentMonth, double orginalLoanPaymentTillNow)
        {
            var j = (double)profitRate / 100;
            double A = (j * 30 * durationOfLoanPaymentMonth * (wholeAmount - orginalLoanPaymentTillNow)) / 365;

            return A;
        }
        public double CalculateRemainLoan(int countOfLoan, double eachLoanAmount, int loanNumber)
        {

            double B = (eachLoanAmount * countOfLoan) - (eachLoanAmount * loanNumber);

            return B;
        }

        public double CalculateFacilityAmount(double wholeAmount, double profitRate, int countOfLoan, int durationOfLoanPayment)
        {
            int per = 365 / countOfLoan;

            double s = profitRate / (per * 100);

            double v = Math.Pow(1 + s, countOfLoan);

            double f = wholeAmount * v;
            double h = v - 1;

            double eachLoanAmount = s * (f / h);
            return eachLoanAmount;

        }

    }



    public class InstalmentResponseModel
    {
        public List<FacilityModel>? FacilityList { get; set; }
        public double FacilityAmount { get; set; }
        public double TotalWholeAmount { get { return FacilityList.Select(s => s.Amount).Sum(); } }
        public double TotalShareAmount => FacilityList.Select(s => s.ShareAmount).Sum();
        public double TotalOriginalAmount => FacilityList.Select(s => s.OrginalAmount).Sum();
    }
    public class FacilityModel
    {
        public byte Id { get; set; }
        public int Amount { get; set; }
        public double ShareAmount { get; set; }
        public double OrginalAmount { get; set; }
        public double RemainOrginalAmount { get; set; }
        public double RemainAmount { get; set; }
        public string? FacilityDate { get; set; }



    }
    public class InstalmentRequestModel
    {
        public double WholeAmount { get; set; }
        public int ProfitRate { get; set; }
        public int CountOfLoan { get; set; }
        public int DurationOfLoanPayment { get; set; }

    }


}
