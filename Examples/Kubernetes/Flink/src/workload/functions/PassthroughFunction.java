package workload.functions;

public class PassthroughFunction extends KeyedProcessFunction<String, CardTransaction, CardTransaction> {
    private static final long serialVersionUID = 1L;

    @Override
    public void processElement(CardTransaction value, Context ctx, Collector<CardTransaction> out) throws Exception {
        out.collect(value);
    }
}
