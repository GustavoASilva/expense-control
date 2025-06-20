FROM mcr.microsoft.com/mssql/server:2022-latest

USER root

# Install curl, apt-transport-https, and gnupg
RUN apt-get update && \
    apt-get install -y curl apt-transport-https gnupg2 && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y msodbcsql18 mssql-tools18 && \
    echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> /etc/bash.bashrc && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

ENV PATH="$PATH:/opt/mssql-tools18/bin"

# Default SQL Server environment variables (can be overridden in docker-compose)
ENV ACCEPT_EULA=Y
ENV SA_PASSWORD=ExpenseControl123!
ENV MSSQL_PID=Developer

EXPOSE 1433

CMD ["/opt/mssql/bin/sqlservr"]
