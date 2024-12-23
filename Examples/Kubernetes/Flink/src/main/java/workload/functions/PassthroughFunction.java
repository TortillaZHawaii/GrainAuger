package workload.functions;

import org.apache.flink.streaming.api.functions.KeyedProcessFunction;
import org.apache.flink.util.Collector;

public class PassthroughFunction extends KeyedProcessFunction<String, CardTransaction, CardTransaction> {
    private static final long serialVersionUID = 1L;

    @Override
    public void processElement(CardTransaction value,
            KeyedProcessFunction<String, CardTransaction, CardTransaction>.Context ctx, Collector<CardTransaction> out)
            throws Exception {
        out.collect(value);
    }
}
