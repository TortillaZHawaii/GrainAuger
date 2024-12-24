package workload.functions;

import java.util.List;

import org.apache.flink.streaming.api.functions.KeyedProcessFunction;
import org.apache.flink.util.Collector;

import com.fasterxml.jackson.databind.ObjectMapper;

public class PalindromeFunction extends KeyedProcessFunction<String, CardTransaction, List<List<String>>> {

    @Override
    public void processElement(CardTransaction value,
            KeyedProcessFunction<String, CardTransaction, List<List<String>>>.Context ctx,
            Collector<List<List<String>>> out) throws Exception {
        Solver solver = new Solver();
        List<List<String>> res = solver.partition(value.getCardNumber());
        out.collect(res);
    }

    public static String toJSON(List<List<String>> res) {
        ObjectMapper mapper = new ObjectMapper();
        try {
            return mapper.writeValueAsString(res);
        } catch (Exception e) {
            e.printStackTrace();
            return "";
        }
    }
}
