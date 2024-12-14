package gofakeit

import (
	"strings"
)

// HackerPhrase will return a random hacker sentence
func HackerPhrase() string { return hackerPhrase(GlobalFaker) }

// HackerPhrase will return a random hacker sentence
func (f *Faker) HackerPhrase() string { return hackerPhrase(f) }

func hackerPhrase(f *Faker) string {
	genStr, _ := generate(f, getRandValue(f, []string{"hacker", "phrase"}))

	words := strings.Split(genStr, " ")
	words[0] = strings.ToUpper(words[0][0:1]) + words[0][1:]
	return strings.Join(words, " ")
}

// HackerAbbreviation will return a random hacker abbreviation
func HackerAbbreviation() string { return hackerAbbreviation(GlobalFaker) }

// HackerAbbreviation will return a random hacker abbreviation
func (f *Faker) HackerAbbreviation() string { return hackerAbbreviation(f) }

func hackerAbbreviation(f *Faker) string {
	return getRandValue(f, []string{"hacker", "abbreviation"})
}

// HackerAdjective will return a random hacker adjective
func HackerAdjective() string { return hackerAdjective(GlobalFaker) }

// HackerAdjective will return a random hacker adjective
func (f *Faker) HackerAdjective() string { return hackerAdjective(f) }

func hackerAdjective(f *Faker) string {
	return getRandValue(f, []string{"hacker", "adjective"})
}

// HackerNoun will return a random hacker noun
func HackerNoun() string { return hackerNoun(GlobalFaker) }

// HackerNoun will return a random hacker noun
func (f *Faker) HackerNoun() string { return hackerNoun(f) }

func hackerNoun(f *Faker) string {
	return getRandValue(f, []string{"hacker", "noun"})
}

// HackerVerb will return a random hacker verb
func HackerVerb() string { return hackerVerb(GlobalFaker) }

// HackerVerb will return a random hacker verb
func (f *Faker) HackerVerb() string { return hackerVerb(f) }

func hackerVerb(f *Faker) string {
	return getRandValue(f, []string{"hacker", "verb"})
}

// HackeringVerb will return a random hacker ingverb
func HackeringVerb() string { return hackeringVerb(GlobalFaker) }

// HackeringVerb will return a random hacker ingverb
func (f *Faker) HackeringVerb() string { return hackeringVerb(f) }

func hackeringVerb(f *Faker) string {
	return getRandValue(f, []string{"hacker", "ingverb"})
}

func addHackerLookup() {
	AddFuncLookup("hackerphrase", Info{
		Display:     "Hacker Phrase",
		Category:    "hacker",
		Description: "Informal jargon and slang used in the hacking and cybersecurity community",
		Example:     "If we calculate the program, we can get to the AI pixel through the redundant XSS matrix!",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return hackerPhrase(f), nil
		},
	})

	AddFuncLookup("hackerabbreviation", Info{
		Display:     "Hacker Abbreviation",
		Category:    "hacker",
		Description: "Abbreviations and acronyms commonly used in the hacking and cybersecurity community",
		Example:     "ADP",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return hackerAbbreviation(f), nil
		},
	})

	AddFuncLookup("hackeradjective", Info{
		Display:     "Hacker Adjective",
		Category:    "hacker",
		Description: "Adjectives describing terms often associated with hackers and cybersecurity experts",
		Example:     "wireless",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return hackerAdjective(f), nil
		},
	})

	AddFuncLookup("hackernoun", Info{
		Display:     "Hacker Noun",
		Category:    "hacker",
		Description: "Noun representing an element, tool, or concept within the realm of hacking and cybersecurity",
		Example:     "driver",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return hackerNoun(f), nil
		},
	})

	AddFuncLookup("hackerverb", Info{
		Display:     "Hacker Verb",
		Category:    "hacker",
		Description: "Verbs associated with actions and activities in the field of hacking and cybersecurity",
		Example:     "synthesize",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return hackerVerb(f), nil
		},
	})

	AddFuncLookup("hackeringverb", Info{
		Display:     "Hackering Verb",
		Category:    "hacker",
		Description: "Verb describing actions and activities related to hacking, often involving computer systems and security",
		Example:     "connecting",
		Output:      "string",
		Generate: func(f *Faker, m *MapParams, info *Info) (any, error) {
			return hackeringVerb(f), nil
		},
	})
}
