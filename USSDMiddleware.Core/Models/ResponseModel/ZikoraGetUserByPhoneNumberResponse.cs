namespace USSDMiddleware.Core.Models.ResponseModel;

public class ZikoraGetUserByPhoneNumberResponse
{
     public string Id { get; set; }
    public string CustomerID { get; set; }
    public string LastName { get; set; }
    public string Gender { get; set; }
    public string GenderString { get; set; }
    public string OtherNames { get; set; }
    public string Address { get; set; }
    public string Email { get; set; }
    public string Landmark { get; set; }
    public string PhoneNumber { get; set; }
    public string NickName { get; set; }
    public string BankVerificationNumber { get; set; }
    public string EmailNotification { get; set; }
    public string PhoneNotification { get; set; }
    public string DistrictOfResidence { get; set; }
    public string DistrictOfBusiness { get; set; }
    public string IdentificationNumber { get; set; }
    public string DateOfBirth { get; set; }
    public string HomeTown { get; set; }
    public string LocalGovernment { get; set; }
    public string MothersMaidenName { get; set; }
    public string Nationale { get; set; }
    public string TestQuestion { get; set; }
    public string TestAnswer { get; set; }
    public string State { get; set; }
    public string WorkAddress { get; set; }
    public int WorkAnnualIncome { get; set; }
    public string WorkEmployerName { get; set; }
    public string WorkNextOfKin { get; set; }
    public string WorkNextOfKinAddress { get; set; }
    public string WorkNextOfKinPhoneNo { get; set; }
    public string WorkNextOfKinRelationship { get; set; }
    public string WorkOccupation { get; set; }
    public string WorkPhoneNo { get; set; }
    public string NameOnUtilityBill { get; set; }
    public string AddressOnUtilityBill { get; set; }
    public string DateOnUtilityBill { get; set; }
    public string CommentsForUtilityBill { get; set; }
    public string IsUtilityBillInfoComplete { get; set; }
    public string NextOfKinEmailAddress { get; set; }
    public string CustomerPhoto { get; set; }
    public string CustomerSignature { get; set; }
   
    public string Name { get; set; }
    public string PostalAddress { get; set; }
    public string BusinessPhoneNo { get; set; }
    public string TaxIDNo { get; set; }
    public string BusinessName { get; set; }
    public string TradeName { get; set; }
    public string IndustrialSector { get; set; }
    public string CompanyRegDate { get; set; }
    public string ContactPersonName { get; set; }
    public string BusinessType { get; set; }
    public string BusinessNature { get; set; }
    public string WebAddress { get; set; }
    public string DateIncorporated { get; set; }
    public string BusinessCommencementDate { get; set; }
    public string RegistrationNumber { get; set; }
    public string CustomerMembers { get; set; }
    public string TheDirectors { get; set; }
    public List<Account> Accounts { get; set; }

    public class Account
    {  
    public string AccessLevel { get; set; }
    public string AccountNumber { get; set; }
    public string AccountStatus { get; set; }
    public string AccountType { get; set; }
    public string AccountBalance { get; set; }
    public string Branch { get; set; }
    public string CustomerID { get; set; }
    public string CustomerName { get; set; }
    public string DateCreated { get; set; }
    public string LastActivityDate { get; set; }
    public string NUBAN { get; set; }
    public string Refree1CustomerID { get; set; }
    public string Refree2CustomerID { get; set; }
    public string ReferenceNo { get; set; }
    }
}