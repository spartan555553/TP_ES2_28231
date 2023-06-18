CREATE TABLE InvestmentFunds (
     id INTEGER PRIMARY KEY REFERENCES Assets(id),
     name VARCHAR(255),
     investment_amount DECIMAL(10, 2),
     default_interest_rate DECIMAL(5, 2)
);