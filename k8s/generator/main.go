package main

import (
	"encoding/json"
	"fmt"
	"os"
	"strconv"

	"github.com/IBM/sarama"
	"github.com/brianvoe/gofakeit/v7"
)

type CardTransaction struct {
	Id         int    `json:"id"`
	CardNumber string `json:"cardNumber"`
	Amount     int    `json:"amount"`
}

func main() {
	var seed uint64 = 1
	amount, err := strconv.Atoi(os.Args[1])
	brokerUrl := os.Args[2]
	cardNumbersCount := amount/5 + 1
	if err != nil {
		panic(err)
	}
	var transactions []CardTransaction
	var cardNumbers []string

	faker := gofakeit.New(seed)

	opts := &gofakeit.CreditCardOptions{
		Types: []string{"visa", "mastercard", "american-express"},
	}
	for i := 0; i < cardNumbersCount; i++ {
		cardNumbers = append(cardNumbers, faker.CreditCardNumber(opts))
	}

	for i := 0; i < amount; i++ {
		transactions = append(transactions, CardTransaction{
			Id:         i,
			CardNumber: cardNumbers[faker.Number(0, cardNumbersCount-1)],
			Amount:     faker.Number(1, 10_000),
		})
	}

	fmt.Println("Total transactions: ", len(transactions))
	fmt.Println("First 5 transactions: ")
	for i := 0; i < min(5, len(transactions)); i++ {
		jsonTransaction, err := json.Marshal(transactions[i])
		if err != nil {
			panic(err)
		}
		fmt.Println(string(jsonTransaction))
	}

	fmt.Printf("Sending transactions to Kafka to topic inputTransactions to broker %s\n", brokerUrl)

	producer, err := sarama.NewSyncProducer([]string{brokerUrl}, nil)
	if err != nil {
		panic(err)
	}
	defer producer.Close()

	var messages []*sarama.ProducerMessage
	for _, transaction := range transactions {
		jsonTransaction, err := json.Marshal(transaction)
		if err != nil {
			panic(err)
		}
		messages = append(messages, &sarama.ProducerMessage{
			Topic: "inputTransactions",
			Value: sarama.ByteEncoder(jsonTransaction),
			Key:   sarama.StringEncoder(transaction.CardNumber),
		})
	}

	batchSize := 10_000
	for i := 0; i < len(messages); i += batchSize {
		end := min(i+batchSize, len(messages))
		fmt.Printf("Sending messages from %d to %d\n", i, end)
		batch := messages[i:end]
		err = producer.SendMessages(batch)
		if err != nil {
			panic(err)
		}
	}

	fmt.Println("All messages sent")
}