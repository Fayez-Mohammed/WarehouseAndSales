public class CustomerReportDto
{
    public int UserNumber { get; set; }
    public string CustomerName { get; set; }
    public string PhoneNumber { get; set; }

    public int TotalOrdersCount { get; set; } 
    public decimal TotalSalesAmount { get; set; } 

  
    public decimal TotalPaid { get; set; } 
    public decimal TotalDebt { get; set; } 
    public DateTime? LastOrderDate { get; set; }
    public int TotalReturnsCount { get;  set; }
    public decimal TotalReturnsAmount { get;  set; }
    public decimal TotalNetDebt { get;  set; }
    public List<UnpaidInvoiceSummaryDto> UnpaidInvoices { get; set; }
    public List<UnpaidInvoiceSummaryDto> UnpaidReturns { get; set; }
   
}

public class UnpaidInvoiceSummaryDto
{
    public int InvoiceCode { get; set; }    
    public decimal OriginalAmount { get; set; } 
    public decimal RemainingAmount { get; set; } 
    public DateTime InvoiceDate { get; set; }
}
public class SalesRepReportDto
{
    public int UserNumber { get; set; }
    public string SalesRepName { get; set; }
    public string PhoneNumber { get; set; }

    public int TotalOrdersCount { get; set; }       
    public decimal TotalSalesVolume { get; set; }    

    public int TotalReturnsCount { get; set; }      
    public decimal TotalReturnsVolume { get; set; }  

    public decimal TotalCommissionEarned { get; set; } 
    public decimal TotalCommissionPaid { get; set; } 
    public decimal TotalCommissionDue { get; set; }   


    public List<UnpaidInvoiceSummaryDto> UnpaidCommissions { get; set; }
}

public class SupplierReportDto
{
    // بيانات المورد
    public int UserNumber { get; set; }
    public string SupplierName { get; set; }
    public string PhoneNumber { get; set; }

    // --- إحصائيات التوريد (البضاعة اللي جبناها منه) ---
    public int TotalSupplyCount { get; set; }        // عدد فواتير الشراء
    public decimal TotalSupplyAmount { get; set; }   // إجمالي قيمة المشتريات
                                                     // --- الموقف المالي ---
    public decimal TotalPaid { get; set; }           // إجمالي الكاش اللي دفعناه له
    public decimal TotalDebt { get; set; }           // إجمالي الديون (قبل خصم المرتجع)
    // --- إحصائيات المرتجعات (البضاعة اللي رجعناها له) ---
    public int TotalReturnsCount { get; set; }       // عدد فواتير المرتجع
    public decimal TotalReturnsAmount { get; set; }  // قيمة المرتجعات (رصيد لينا)



    // ⚠️ صافي المستحق للمورد: (ديون الشراء - مستحقات المرتجع)
    public decimal TotalNetDebt { get; set; }

    // القوائم التفصيلية
    // 1. فواتير شراء لسه مادفعنهاش (فلوس علينا)
    public List<UnpaidInvoiceSummaryDto> UnpaidInvoices { get; set; }

    // 2. مرتجعات لسه ماتخصمتش من الحساب (فلوس لينا/رصيد)
    public List<UnpaidInvoiceSummaryDto> UnpaidReturns { get; set; }
}