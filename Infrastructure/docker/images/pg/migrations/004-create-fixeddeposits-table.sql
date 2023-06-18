CREATE TABLE FixedDeposits (
   id INTEGER PRIMARY KEY REFERENCES Assets(id),
   value DECIMAL(10, 2),
   bank VARCHAR(255),
   account_number VARCHAR(255),
   account_holders VARCHAR(255),
   annual_interest_rate DECIMAL(5, 2)
);