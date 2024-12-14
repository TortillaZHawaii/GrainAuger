package gofakeit

// Adverb will generate a random adverb
func Adverb() string { return adverb(GlobalFaker) }

// Adverb will generate a random adverb
func (f *Faker) Adverb() string { return adverb(f) }

func adverb(f *Faker) string {
	var adverbType = map[int]string{
		0: "adverb_manner",
		1: "adverb_degree",
		2: "adverb_place",
		3: "adverb_time_definite",
		4: "adverb_time_indefinite",
		5: "adverb_frequency_definite",
		6: "adverb_frequency_indefinite",
	}
	return getRandValue(f, []string{"word", adverbType[number(f, 0, 6)]})
}

// AdverbManner will generate a random manner adverb
func AdverbManner() string { return adverbManner(GlobalFaker) }

// AdverbManner will generate a random manner adverb
func (f *Faker) AdverbManner() string { return adverbManner(f) }

func adverbManner(f *Faker) string { return getRandValue(f, []string{"word", "adverb_manner"}) }

// AdverbDegree will generate a random degree adverb
func AdverbDegree() string { return adverbDegree(GlobalFaker) }

// AdverbDegree will generate a random degree adverb
func (f *Faker) AdverbDegree() string { return adverbDegree(f) }

func adverbDegree(f *Faker) string { return getRandValue(f, []string{"word", "adverb_degree"}) }

// AdverbPlace will generate a random place adverb
func AdverbPlace() string { return adverbPlace(GlobalFaker) }

// AdverbPlace will generate a random place adverb
func (f *Faker) AdverbPlace() string { return adverbPlace(f) }

func adverbPlace(f *Faker) string { return getRandValue(f, []string{"word", "adverb_place"}) }

// AdverbTimeDefinite will generate a random time definite adverb
func AdverbTimeDefinite() string { return adverbTimeDefinite(GlobalFaker) }

// AdverbTimeDefinite will generate a random time definite adverb
func (f *Faker) AdverbTimeDefinite() string { return adverbTimeDefinite(f) }

func adverbTimeDefinite(f *Faker) string {
	return getRandValue(f, []string{"word", "adverb_time_definite"})
}

// AdverbTimeIndefinite will generate a random time indefinite adverb
func AdverbTimeIndefinite() string { return adverbTimeIndefinite(GlobalFaker) }

// AdverbTimeIndefinite will generate a random time indefinite adverb
func (f *Faker) AdverbTimeIndefinite() string { return adverbTimeIndefinite(f) }

func adverbTimeIndefinite(f *Faker) string {
	return getRandValue(f, []string{"word", "adverb_time_indefinite"})
}

// AdverbFrequencyDefinite will generate a random frequency definite adverb
func AdverbFrequencyDefinite() string { return adverbFrequencyDefinite(GlobalFaker) }

// AdverbFrequencyDefinite will generate a random frequency definite adverb
func (f *Faker) AdverbFrequencyDefinite() string { return adverbFrequencyDefinite(f) }

func adverbFrequencyDefinite(f *Faker) string {
	return getRandValue(f, []string{"word", "adverb_frequency_definite"})
}

// AdverbFrequencyIndefinite will generate a random frequency indefinite adverb
func AdverbFrequencyIndefinite() string { return adverbFrequencyIndefinite(GlobalFaker) }

// AdverbFrequencyIndefinite will generate a random frequency indefinite adverb
func (f *Faker) AdverbFrequencyIndefinite() string { return adverbFrequencyIndefinite(f) }

func adverbFrequencyIndefinite(f *Faker) string {
	return getRandValue(f, []string{"word", "adverb_frequency_indefinite"})
}

func addWordAdverbLookup() {
	AddFuncLookup("adverb", Info{
		Display:     "Adverb",
		Category:    "word",
		Description: "Word that modifies verbs, adjectives, or other adverbs",
		Example:     "smoothly",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return adverb(f), nil
		},
	})

	AddFuncLookup("adverbmanner", Info{
		Display:     "Adverb Manner",
		Category:    "word",
		Description: "Adverb that describes how an action is performed",
		Example:     "stupidly",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return adverbManner(f), nil
		},
	})

	AddFuncLookup("adverbdegree", Info{
		Display:     "Adverb Degree",
		Category:    "word",
		Description: "Adverb that indicates the degree or intensity of an action or adjective",
		Example:     "intensely",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return adverbDegree(f), nil
		},
	})

	AddFuncLookup("adverbplace", Info{
		Display:     "Adverb Place",
		Category:    "word",
		Description: "Adverb that indicates the location or direction of an action",
		Example:     "east",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return adverbPlace(f), nil
		},
	})

	AddFuncLookup("adverbtimedefinite", Info{
		Display:     "Adverb Time Definite",
		Category:    "word",
		Description: "Adverb that specifies the exact time an action occurs",
		Example:     "now",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return adverbTimeDefinite(f), nil
		},
	})

	AddFuncLookup("adverbtimeindefinite", Info{
		Display:     "Adverb Time Indefinite",
		Category:    "word",
		Description: "Adverb that gives a general or unspecified time frame",
		Example:     "already",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return adverbTimeIndefinite(f), nil
		},
	})

	AddFuncLookup("adverbfrequencydefinite", Info{
		Display:     "Adverb Frequency Definite",
		Category:    "word",
		Description: "Adverb that specifies how often an action occurs with a clear frequency",
		Example:     "hourly",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return adverbFrequencyDefinite(f), nil
		},
	})

	AddFuncLookup("adverbfrequencyindefinite", Info{
		Display:     "Adverb Frequency Indefinite",
		Category:    "word",
		Description: "Adverb that specifies how often an action occurs without specifying a particular frequency",
		Example:     "occasionally",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return adverbFrequencyIndefinite(f), nil
		},
	})
}
