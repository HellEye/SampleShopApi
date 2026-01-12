# Sample Shop API

This is a small example shop API made in ASP.NET.
It handles CRUD operations on songs, as well as managing a cart session.

## Usage

### Local

1. Add a .env file with `DATABASE_URL` containing the connection string for a postgresql instance
2. run `dotnet run` to start the dev server

### Deployment

The app is also deployed on railway, using the built in env management and internal project connections

## API Endpoints

All endpoints are visible using stoplightio elements
Visit `/docs` to see the UI, or `http://localhost:5261/openapi/v1.json` to get the full openapi spec
