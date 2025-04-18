namespace VisualAmeco.Parser.Utilities;

public static class SubchapterToChapterMap
{
    public static Dictionary<string, string> Mapping = new Dictionary<string, string>
    {
        // Chapter 1: Population And Employment
        { "01 Population", "Population And Employment" },
        { "02 Labour Force Statistics", "Population And Employment" },
        { "03 Unemployment", "Population And Employment" },
        { "04 Employment, Persons (National Accounts)", "Population And Employment" },
        { "05 Employment, Full-Time Equivalents (National Accounts)", "Population And Employment" },
        { "06 Self-Employed, Persons (National Accounts)", "Population And Employment" },
        { "07 Wage And Salary Earners, Persons (National Accounts)", "Population And Employment" },
        { "08 Wage And Salary Earners, Full-Time Equivalents (National Acc...", "Population And Employment" },

        // Chapter 2: Consumption
        { "01 Private Final Consumption Expenditure", "Consumption" },
        { "02 Private Final Consumption Expenditure Per Head Of Population", "Consumption" },
        { "03 Actual Individual Final Consumption Of Households", "Consumption" },
        { "04 Consumer Price Index", "Consumption" },
        { "05 Total Final Consumption Expenditure Of General Government", "Consumption" },
        { "06 Collective Consumption Expenditure Of General Government", "Consumption" },
        { "07 Individual Consumption Expenditure Of General Government", "Consumption" },
        { "08 Total Consumption", "Consumption" },

        // Chapter 3: Capital Formation And Saving, Total Economy And Sectors
        { "01 Gross Fixed Capital Formation, Total Economy", "Capital Formation And Saving" },
        { "02 Gross Fixed Capital Formation At Current Prices, Sectors", "Capital Formation And Saving" },
        { "03 Net Fixed Capital Formation, Total Economy", "Capital Formation And Saving" },
        { "04 Net Fixed Capital Formation At Current Prices, Sectors", "Capital Formation And Saving" },
        { "05 Consumption Of Fixed Capital, Total Economy", "Capital Formation And Saving" },
        { "06 Consumption Of Fixed Capital, General Government", "Capital Formation And Saving" },
        { "07 Gross Fixed Capital Formation By Type Of Goods At Current Prices", "Capital Formation And Saving" },
        { "08 Gross Fixed Capital Formation By Type Of Goods At Constant Prices", "Capital Formation And Saving" },
        { "09 Gross Fixed Capital Formation By Type Of Goods, Price Deflators", "Capital Formation And Saving" },
        { "10 Change In Inventories And Net Acquisition Of Valuables", "Capital Formation And Saving" },
        { "11 Gross Capital Formation", "Capital Formation And Saving" },
        { "12 Gross Saving", "Capital Formation And Saving" },
        { "13 Net Saving", "Capital Formation And Saving" },

        // Chapter 4: Domestic And Final Demand
        { "01 Domestic Demand Excluding Change In Inventories", "Domestic And Final Demand" },
        { "02 Domestic Demand Including Change In Inventories", "Domestic And Final Demand" },
        { "03 Final Demand", "Domestic And Final Demand" },
        { "04 Contributions To The Change Of The Final Demand Deflator", "Domestic And Final Demand" },

        // Chapter 5: National Income
        { "01 Gross National Income", "National Income" },
        { "02 Gross National Income Per Head Of Population", "National Income" },
        { "03 Net National Income", "National Income" },
        { "04 National Disposable Income At Current Prices", "National Income" },
        { "05 Gross National Disposable Income Per Head Of Population", "National Income" },

        // Chapter 6: Domestic Product
        { "01 Gross Domestic Product", "Domestic Product" },
        { "02 Gross Domestic Product Per Head Of Population", "Domestic Product" },
        { "03 Gross Domestic Product Per Person Employed", "Domestic Product" },
        { "04 Gross Domestic Product Per Hour Worked", "Domestic Product" },
        { "05 Potential Gross Domestic Product At Constant Prices", "Domestic Product" },
        { "06 Trend Gross Domestic Product At Constant Prices", "Domestic Product" },
        { "07 Gdp At Constant Prices Adjusted For The Impact Of Terms Of Trade Per Head", "Domestic Product" },
        { "08 Contributions To The Change Of Gdp At Constant Market Prices", "Domestic Product" },
        { "09 Alternative Definitions Domestic Product At Current Prices", "Domestic Product" },
        { "10 Gross Value Added, Total Economy", "Domestic Product" },

        // Chapter 7: Gross Domestic Product (Income Approach), Labour Costs
        { "01 Compensation Of Employees", "Gross Domestic Product (Income Approach)" },
        { "02 Taxes Linked To Imports And Production And Subsidies; Total Economy", "Gross Domestic Product (Income Approach)" },
        { "03 Operating Surplus, Total Economy", "Gross Domestic Product (Income Approach)" },
        { "04 Nominal Compensation Per Employee, Total Economy", "Gross Domestic Product (Income Approach)" },
        { "05 Real Compensation Per Employee, Total Economy", "Gross Domestic Product (Income Approach)" },
        { "06 Adjusted Wage Share", "Gross Domestic Product (Income Approach)" },
        { "07 Nominal Unit Labour Costs, Total Economy", "Gross Domestic Product (Income Approach)" },
        { "08 Real Unit Labour Costs, Total Economy", "Gross Domestic Product (Income Approach)" },

        // Chapter 8: Capital Stock
        { "01 Net Capital Stock At Constant Prices, Total Economy", "Capital Stock" },
        { "02 Factor Productivity, Total Economy", "Capital Stock" },
        { "03 Production Factors Substitution, Total Economy", "Capital Stock" },
        { "04 Marginal Efficiency Of Capital, Total Economy", "Capital Stock" },

        // Chapter 9: Exports And Imports Of Goods And Services
        { "01 Exports Of Goods And Services", "Exports And Imports" },
        { "02 Imports Of Goods And Services", "Exports And Imports" },
        { "03 Exports Of Goods", "Exports And Imports" },
        { "04 Exports Of Services", "Exports And Imports" },
        { "05 Imports Of Goods", "Exports And Imports" },
        { "06 Imports Of Services", "Exports And Imports" },
        { "07 Terms Of Trade", "Exports And Imports" },

        // Chapter 10: Balances With The Rest Of The World, National Accounts
        { "01 Balances With The Rest Of The World, National Accounts", "Balances With The Rest Of The World" },
        { "02 Balances With The Rest Of The World, Bop Statistics", "Balances With The Rest Of The World" },

        // Chapter 11: Foreign Trade At Current Prices
        { "01 Foreign Trade At Current Prices", "Foreign Trade" },
        { "02 Foreign Trade Shares In World Trade", "Foreign Trade" },

        // Chapter 12: National Accounts By Branch Of Activity
        { "01 Employment, Persons", "National Accounts By Branch Of Activity" },
        { "02 Employment, Full-Time Equivalents", "National Accounts By Branch Of Activity" },
        { "03 Wage And Salary Earners, Persons", "National Accounts By Branch Of Activity" },
        { "04 Wage And Salary Earners, Full-Time Equivalents", "National Accounts By Branch Of Activity" },
        { "05 Gross Value Added By Main Branch At Current Prices", "National Accounts By Branch Of Activity" },
        { "06 Gross Value Added By Main Branch At Current Prices Per Person Employed", "National Accounts By Branch Of Activity" },
        { "07 Gross Value Added By Main Branch At Current Prices Per Employee", "National Accounts By Branch Of Activity" },
        { "08 Gross Value Added By Main Branch At Constant Prices", "National Accounts By Branch Of Activity" },
        { "09 Gross Value Added By Main Branch At Constant Prices Per Person Employed", "National Accounts By Branch Of Activity" },
        { "10 Gross Value Added By Main Branch At Constant Prices Per Employee", "National Accounts By Branch Of Activity" },
        { "11 Price Deflator Gross Value Added By Main Branch", "National Accounts By Branch Of Activity" },
        { "12 Compensation Of Employees By Main Branch", "National Accounts By Branch Of Activity" },
        { "13 Nominal Compensation By Main Branch Per Employee", "National Accounts By Branch Of Activity" },
        { "14 Adjusted Wage Share By Main Branch", "National Accounts By Branch Of Activity" },
        { "15 Nominal Unit Wage Costs By Main Branch", "National Accounts By Branch Of Activity" },
        { "16 Nominal Unit Labour Costs By Main Branch", "National Accounts By Branch Of Activity" },
        { "17 Real Unit Labour Costs By Main Branch", "National Accounts By Branch Of Activity" },
        { "18 Industrial Production", "National Accounts By Branch Of Activity" },

        // Chapter 13: Monetary Variables
        { "01 Exchange Rates And Purchasing Power Parities", "Monetary Variables" },
        { "02 Interest Rates", "Monetary Variables" },

        // Chapter 14: Corporations (S11 + S12)
        { "01 Revenue", "Corporations" },
        { "02 Expenditure", "Corporations" },
        { "03 Balances", "Corporations" },

        // Chapter 15: Households And Npish (S14 + S15)
        { "01 Revenue", "Households And Npish" },
        { "02 Expenditure", "Households And Npish" },
        { "03 Balances", "Households And Npish" },

        // Chapter 16: General Government (S13)
        { "01 Revenue (Esa 2010)", "General Government" },
        { "02 Expenditure (Esa 2010)", "General Government" },
        { "03 Net Lending (Esa 2010)", "General Government" },
        { "04 Excessive Deficit Procedure", "General Government" },

        // Chapter 17: Cyclical Adjustment Of Public Finance Variables
        { "01 Based On Potential Gdp (Esa 2010)", "Cyclical Adjustment Of Public Finance Variables" },
        { "02 Based On Trend Gdp (Esa 2010)", "Cyclical Adjustment Of Public Finance Variables" },

        // Chapter 18: Gross Public Debt
        { "01 Based On Esa 2010", "Gross Public Debt" },
        { "02 Based On Esa 2010 And Former Definitions (Linked Series)", "Gross Public Debt" }
    };
}