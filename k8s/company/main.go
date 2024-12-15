package main

import (
	"fmt"
	"net/http"
	"regexp"
)

func getCompanyName(cardNumber string) string {
	regexes := map[string]string{
		"visa":             "^4[0-9]{12}(?:[0-9]{3})?$",
		"mastercard":       "^5[1-5][0-9]{14}$",
		"american-express": "^3[47][0-9]{13}$",
	}

	for company, regex := range regexes {
		if match, _ := regexp.MatchString(regex, cardNumber); match {
			return company
		}
	}

	return "unknown"
}

func companyHandler(w http.ResponseWriter, r *http.Request) {
	cardNumber := r.URL.Query().Get("number")
	if cardNumber == "" {
		http.Error(w, "number is required", http.StatusBadRequest)
		return
	}

	companyName := getCompanyName(cardNumber)

	w.Write([]byte(companyName))
}

func main() {
	http.HandleFunc("/company", companyHandler)
	fmt.Println("Server is running on port 8080")
	http.ListenAndServe(":8080", nil)
	fmt.Println("Server is stopped")
}
