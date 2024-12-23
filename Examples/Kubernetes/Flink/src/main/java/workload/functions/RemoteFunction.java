package workload.functions;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;

import org.apache.flink.streaming.api.functions.KeyedProcessFunction;
import org.apache.flink.util.Collector;

public class RemoteFunction extends KeyedProcessFunction<String, CardTransaction, CardCompany> {
    @Override
    public void processElement(CardTransaction value,
            KeyedProcessFunction<String, CardTransaction, CardCompany>.Context ctx, Collector<CardCompany> out)
            throws Exception {
        String url = "http://company-server-svc/company?number=" + value.getCardNumber();
        var uri = URI.create(url);
        var request = HttpRequest.newBuilder()
            .uri(uri)
            .GET()
            .build();
        var response = HttpClient.newHttpClient().send(request, HttpResponse.BodyHandlers.ofString());

        if (response.statusCode() != 200) {
            throw new Exception("Failed to fetch company for card number " + value.getCardNumber());
        }

        var company = new CardCompany();
        company.setCompany(response.body());
        company.setTransaction(value);

        out.collect(company);
    }
}
