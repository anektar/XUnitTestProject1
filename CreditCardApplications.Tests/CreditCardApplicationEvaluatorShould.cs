#region Using
using System;
using Xunit;
using Moq;
using Moq.Protected;
using System.Collections.Generic;
#endregion

namespace CreditCardApplications.Tests
{
    public class CreditCardApplicationEvaluatorShould
    {
        #region Variables
        Mock<IFrequentFlyerNumberValidator> _mockValidator;
        CreditCardApplicationEvaluator _sut;
        bool _isValid = true;

        //Testing Hierarchy
        Mock<ILicenseData> _mockLicenseData;
        Mock<IServiceInformation> _mockServiceInformation;
        #endregion

        #region Constructor
        public CreditCardApplicationEvaluatorShould()
        {
            #region Instantiate _mockValidator without hierarcy
            //_mockValidator = new Mock<IFrequentFlyerNumberValidator>();

            //If Setting to MockBehavior.Strict then all methods called must be previously Setup
            //_mockValidator = new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Strict);

            //If Setting to MockBehavior.Loose ==  MockBehavior.Default then all methods not previously Setup
            //return default values
            _mockValidator = new Mock<IFrequentFlyerNumberValidator>(MockBehavior.Loose);
            #endregion

            #region 1st Way to Instantiate _mockValidator objects with hierarcy
            //_mockLicenseData = new Mock<ILicenseData>();
            //_mockServiceInformation = new Mock<IServiceInformation>();

            //_mockServiceInformation.Setup(x => x.License).Returns(_mockLicenseData.Object); //instantied four lines above
            //_mockValidator.Setup(x => x.ServiceInformation).Returns(_mockServiceInformation.Object);
            #endregion

            #region 2nd Way to Instantiate _mockValidator objects with hierarcy, Set DefaultValue.Mock
            //This will make all non instantiated objects that can be mocked (Interfaces, Abstract Classes or non-sealed Classes
            _mockValidator.DefaultValue = DefaultValue.Mock;
            #endregion

            #region Possible Setups
            //_mockValidator.Setup(x => x.IsValid("x")).Returns(true);
            //_mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            //_mockValidator.Setup(x => x.IsValid(It.Is<string>(number => number.StartsWith("y")))).Returns(true);
            //_mockValidator.Setup(x => x.IsValid(It.Is<string>(x => x.Contains("z")))).Returns(true);
            //_mockValidator.Setup(x => x.IsValid(It.Is<string>( ffn => ffn.EndsWith("z")))).Returns(true);
            //_mockValidator.Setup(x => x.IsValid(It.IsInRange<string>("a","z", Moq.Range.Inclusive))).Returns(true);
            //_mockValidator.Setup(x => x.IsValid(It.IsIn("a", "x", "y"))).Returns(true);

            //All Ok if not Strict
            //_mockValidator.Setup(x => x.IsValid(It.IsRegex(@"^[a-zA-Z0-9_]+$"))).Returns(true);

            //Needed for DeclineLowIncomeApplicationsOut
            //_mockValidator.Setup(x => x.IsValid(It.IsAny<string>(), out _isValid));

            //Example with ref parameters
            //_mockValidator.Setup(x => x.IsValid(It.Ref<string>.IsAny)).Returns(true);
            #endregion

            //All Ok if Strict or not strict
            //_mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);
            
            //After changing IsValid to Generic type we can have the following to work
            _mockValidator.Setup(x => x.IsValid(It.IsAny<It.IsAnyType>())).Returns(true);


            //Instantiate the System Under Test
            _sut = new CreditCardApplicationEvaluator(_mockValidator.Object);
        }
        #endregion

        #region Helper Methods
        string GetLicenseKeyExpiryString()
        {
            // E.g. read from vendor-supplied constants file
            return "EXPIRED";
        }
        #endregion

        #region Tests

        #region State Test Methods
        [Fact]
        [Trait("Category", "1.State Tests")]
        public void AcceptHighIncomeApplications()
        {
            CreditCardApplication application = new CreditCardApplication
            {
                GrossAnnualIncome = 100_000
            };

            CreditCardApplicationDecision decision = _sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoAccepted, decision);
        }

        [Fact]
        [Trait("Category", "1.State Tests")]
        public void ReferYoungApplications()
        {
            CreditCardApplication application = new CreditCardApplication
            {
                Age = 19
            };
            CreditCardApplicationDecision decision = _sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        [Trait("Category", "1.State Tests")]
        public void DeclineLowIncomeApplications()
        {
            CreditCardApplication application = new CreditCardApplication
            {
                GrossAnnualIncome = 19_999,
                Age = 42,
                //FrequentFlyerNumber = "#@!"
                FrequentFlyerNumber = "lkdfljkfdljk2"
            };
            CreditCardApplicationDecision decision = _sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        [Trait("Category", "1.State Tests")]
        public void ReferInvalidFrequentFlyerApplications()
        {
            CreditCardApplication application = new CreditCardApplication
            {
            };
            CreditCardApplicationDecision decision = _sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        [Trait("Category", "1.State Tests")]
        public void DeclineLowIncomeApplicationsOut()
        {
            CreditCardApplication application = new CreditCardApplication
            {
                GrossAnnualIncome = 19_999,
                Age = 42
            };

            //Since this is the only one using the EvaluateUsingOut I only need it's setup here.
            //Ofcourse I can put it in the test class constructor for better code organization
            _mockValidator.Setup(x => x.IsValid(It.IsAny<string>(), out _isValid));

            CreditCardApplicationDecision decision = _sut.EvaluateUsingOut(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        [Fact]
        [Trait("Category", "1.State Tests")]
        public void ReferWhenLicenseKeyExpired()
        {
            CreditCardApplication application = new CreditCardApplication
            {
                Age = 42
            };

            //Testing with simple LicenseKey Property
            //_mockValidator.Setup(x => x.LicenseKey).Returns("EXPIRED");
            //_mockValidator.Setup(x => x.LicenseKey).Returns(GetLicenseKeyExpiryString);

            //Testing with LicenseKey Property under hierarcy
            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns(GetLicenseKeyExpiryString);

            CreditCardApplicationDecision decision = _sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }

        [Fact]
        [Trait("Category", "1.State Tests")]
        public void UseDetailedLookupForOlderApplications()
        {
            CreditCardApplication application = new CreditCardApplication
            {
                Age = 30
            };

            /*Mocked properties by default to not remember changes.
             * This means that the following line:
             * 
             * _validator.ValidationMode = 
             * application.Age >= 30 ? ValidationMode.Detailed : ValidationMode.Quick;
             * 
             * inside the Evaluate method which sets the .Age property does not get
             * to update the .Age property if we don't explicitly setup this property or all properties
             */

            //In order to do so, we either use the following 
            _mockValidator.SetupProperty(x => x.ValidationMode);

            //or we enable all properties to remember changes
            //_mockValidator.SetupAllProperties();

            //SetupAllProperties must be called PRIOR to any specific property setups, i.e. before the line below
            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            _sut.Evaluate(application);

            Assert.Equal(ValidationMode.Detailed, _mockValidator.Object.ValidationMode);
        }




        #endregion

        #region Behavioral Test Methods
        /// <summary>
        /// Behavioral Test. We want to see if the IsValid Method of the mocked IFrequentFlyerNumberValidator is called
        /// </summary>
        [Fact]
        [Trait("Category", "2.Behavioral Tests")]
        public void ValidateFrequentFlyerNumberForLowIncomeApplications()
        {
            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            CreditCardApplication application = new CreditCardApplication
            {
                FrequentFlyerNumber = "q"
            };

            _sut.Evaluate(application);

            //Following works if FrequentFlyerNumber of CreditCardApplication is not set
            //_mockValidator.Verify(x => x.IsValid(null));

            //Following will check if IsValid method is called at least once with the parameter value q
            //_mockValidator.Verify(x => x.IsValid("q"));

            //Following will check if IsValid method is called at least once with any string parameter 
            _mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), "Frequent flyer numbers should be validated");
        }

        [Fact]
        [Trait("Category", "2.Behavioral Tests")]
        public void NotValidateFrequentFlyerNumberForHighIncomeApplications()
        {
            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            CreditCardApplication application = new CreditCardApplication
            {
                GrossAnnualIncome = 100_000
            };

            _sut.Evaluate(application);

            _mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Never, "HighIncomeApplications should not be validated");
        }

        [Fact]
        [Trait("Category", "2.Behavioral Tests")]
        public void ValidateOnceFrequentFlyerNumberForLowIncomeApplications()
        {
            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            CreditCardApplication application = new CreditCardApplication
            {
                FrequentFlyerNumber = "q"
            };

            _sut.Evaluate(application);

            //Following works if FrequentFlyerNumber of CreditCardApplication is not set
            //_mockValidator.Verify(x => x.IsValid(null));

            //Following will check if IsValid method is called at least once with the parameter value q
            //_mockValidator.Verify(x => x.IsValid("q"));

            //Following will check if IsValid method is called at least once with any string parameter 
            _mockValidator.Verify(x => x.IsValid(It.IsAny<string>()), Times.Exactly(1), "Frequent flyer numbers should be validated exactly once.");
        }

        [Fact]
        [Trait("Category", "2.Behavioral Tests")]
        public void CheckLicenseKeyForLowIncomeApplications()
        {
            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            CreditCardApplication application = new CreditCardApplication
            {
                GrossAnnualIncome = 99_000
            };

            _sut.Evaluate(application);

            _mockValidator.VerifyGet(x => x.ServiceInformation.License.LicenseKey, Times.Once, "License Key should be read (get) once for Low Income Applications.");
        }

        [Fact]
        [Trait("Category", "2.Behavioral Tests")]
        public void SetDetailedLookupForOlderApplications()
        {
            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            CreditCardApplication application = new CreditCardApplication
            {
                Age = 30

            };

            _sut.Evaluate(application);

            _mockValidator.VerifySet(x => x.ValidationMode = ValidationMode.Detailed, Times.Once, "Validation mode should be set once for Older Applications.");
            //_mockValidator.VerifySet(x => x.ValidationMode = It.IsAny<ValidationMode>(), Times.Once, "Validation mode should be set once for Older Applications.");

            _mockValidator.Verify(x => x.IsValid<string>(null), Times.Once, "Validation mode should be called once.");

            //_mockValidator.VerifyNoOtherCalls();
        }
        #endregion

        #region Exception Test Methods
        [Fact]
        [Trait("Category", "3.Exception Thrown Tests")]
        public void ReferWhenFrequentFlyerValidationError()
        {
            CreditCardApplication application = new CreditCardApplication
            {
                Age = 42
            };

            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            //_mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Throws<Exception>();
            _mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Throws(new Exception("Custom message"));

            CreditCardApplicationDecision decision = _sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, decision);
        }
        #endregion

        #region Check that Events are raised Test Methods
        [Fact]
        [Trait("Category", "4.Events Raised Tests")]
        public void IncrementLookupCount()
        {
            CreditCardApplication application = new CreditCardApplication
            {
                FrequentFlyerNumber = "x",
                Age = 25
            };

            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            //This will cause the event to be called in the mock object
            //_mockValidator.Raise(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            //additionally we could do it in constructor chaining it to existing Setup of is Valid below
            _mockValidator.Setup(x => x.IsValid(It.IsAny<string>()))
                          .Returns(true)
                          .Raises(x => x.ValidatorLookupPerformed += null, EventArgs.Empty);

            _sut.Evaluate(application);

            Assert.Equal(1, _sut.ValidatorLookupCount);
        }
        #endregion

        #region Check That Methods return different values if called repeatedly
        [Fact]
        [Trait("Category", "5.Repeated Calls")]
        public void ReferInvalidFrequentFlyerApplications_ReturnValuesSequence()
        {
            CreditCardApplication application = new CreditCardApplication
            {
                Age = 42
            };

            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            //_mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(false);
            _mockValidator.SetupSequence(x => x.IsValid(It.IsAny<string>()))
                .Returns(false)
                .Returns(true);

            CreditCardApplicationDecision firstDecision = _sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.ReferredToHuman, firstDecision);

            CreditCardApplicationDecision secondDecision = _sut.Evaluate(application);
            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, secondDecision);
        }

        [Fact]
        [Trait("Category", "5.Repeated Calls")]
        public void ReferInvalidFrequentFlyerApplications_MultipleCallsSequence()
        {
            _mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");

            List<string> frequentFlyerNumbersPassed = new List<string>();
            _mockValidator.Setup(x => x.IsValid(Capture.In(frequentFlyerNumbersPassed)));

            CreditCardApplication application1 = new CreditCardApplication { Age = 42, FrequentFlyerNumber = "aa" };
            CreditCardApplication application2 = new CreditCardApplication { Age = 42, FrequentFlyerNumber = "bb" };
            CreditCardApplication application3 = new CreditCardApplication { Age = 42, FrequentFlyerNumber = "cc" };

            _sut.Evaluate(application1);
            _sut.Evaluate(application2);
            _sut.Evaluate(application3);

            //Assert that IsValid was called three times with "aa", "bb" and "cc"
            Assert.Equal(new List<string> { "aa", "bb", "cc" }, frequentFlyerNumbersPassed);
        }
        #endregion

        #region Partial Mocks
        [Fact]
        [Trait("Category", "6.Partial Mock Tests")]
        public void ReferFraudRisk()
        {
            Mock<FraudLookup> mockFraudLookup = new Mock<FraudLookup>();
            //We need to Setup the IsFraudRisk To return true for the test to suceed
            //mockFraudLookup.Setup(x => x.IsFraudRisk(It.IsAny<CreditCardApplication>())).Returns(true);

            //previous works if IsFraudRisk is virtual. If Not then we need the following, along with 
            //using Moq.Protected 
            //in Usings
            mockFraudLookup.Protected()
                           .Setup<bool>("CheckApplication", ItExpr.IsAny<CreditCardApplication>()).Returns(true);



            CreditCardApplicationEvaluator sut = new CreditCardApplicationEvaluator(_mockValidator.Object, mockFraudLookup.Object);

            CreditCardApplication application = new CreditCardApplication {};
            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.ReferredToHumanFraudRisk, decision);
        }

        #endregion

        #region LinqToMoq support
        [Fact]
        [Trait("Category", "7.LinqToMock Tests")]
        public void LinqToMocks()
        {
            //Fluent Syntax
            //_mockValidator.Setup(x => x.ServiceInformation.License.LicenseKey).Returns("OK");
            //_mockValidator.Setup(x => x.IsValid(It.IsAny<string>())).Returns(true);

            //LinqToMock Syntax
            IFrequentFlyerNumberValidator mockValidator = Mock.Of<IFrequentFlyerNumberValidator>
                (
                    validator =>
                    validator.ServiceInformation.License.LicenseKey == "OK" &&
                    validator.IsValid(It.IsAny<string>()) == true
                );

            CreditCardApplication application = new CreditCardApplication 
            { 
                Age = 25
            };
            //CreditCardApplicationDecision decision = _sut.Evaluate(application);

            CreditCardApplicationEvaluator sut = new CreditCardApplicationEvaluator(mockValidator);
            CreditCardApplicationDecision decision = sut.Evaluate(application);

            Assert.Equal(CreditCardApplicationDecision.AutoDeclined, decision);
        }

        #endregion

        #endregion
    }
}
