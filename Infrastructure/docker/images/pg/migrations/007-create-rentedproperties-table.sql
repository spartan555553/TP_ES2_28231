CREATE TABLE RentedProperties (
  id INTEGER PRIMARY KEY REFERENCES Assets(id),
  name VARCHAR(255),
  location VARCHAR(255),
  property_value DECIMAL(10, 2),
  rental_value DECIMAL(10, 2),
  monthly_condominium_fee DECIMAL(10, 2),
  estimated_annual_expenses DECIMAL(10, 2)
);