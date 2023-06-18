CREATE TABLE MonthlyInterestRates (
  id SERIAL PRIMARY KEY,
  fund_id INTEGER REFERENCES InvestmentFunds(id),
  month DATE,
  interest_rate DECIMAL(5, 2)
);