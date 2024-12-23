package workload.functions;

import com.fasterxml.jackson.annotation.JsonProperty;
import com.fasterxml.jackson.databind.ObjectMapper;

public class CardCompany 
{
    @JsonProperty("company")
    private String company;

    @JsonProperty("transaction")
    private CardTransaction transaction;

    // Getters and Setters
    public String getCompany() {
        return company;
    }

    public void setCompany(String company) {
        this.company = company;
    }

    public CardTransaction getTransaction() {
        return transaction;
    }

    public void setTransaction(CardTransaction transaction) {
        this.transaction = transaction;
    }

    public String toJSON() {
        ObjectMapper objectMapper = new ObjectMapper();
        try {
            return objectMapper.writeValueAsString(this);
        } catch (Exception e) {
            e.printStackTrace();
            return null;
        }
    }
}
