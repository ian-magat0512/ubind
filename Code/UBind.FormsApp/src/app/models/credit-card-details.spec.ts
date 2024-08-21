import { CreditCardDetails } from './credit-card-details';

describe('CreditCardDetails', () => {

    beforeEach(() => {
    });

    it('expiryYearYYYY should return four digit year in 21st century.', () => {
        // Arrange
        let sut: CreditCardDetails = new CreditCardDetails(
            "John Smith",
            "4242424242424242",
            "12/30",
            "123");

        // Act
        let year: string = sut.expiryYearYYYY;

        // Assert
        expect(year).toBe("2030");
    });

    it('cardNumber should return the correct cardnumber numeric value.', () => {
        // Arrange
        let sut: CreditCardDetails = new CreditCardDetails(
            "John Smith",
            "2223003122003222__ __ __ __",
            "12/30",
            "123");

        // Act
        let cardNumber: string = sut.number;

        // Assert
        expect(cardNumber).toBe("2223003122003222");
    });

    it('cardExpiryDateMMYY should return the correct expiryDate numeric value.', () => {
        // Arrange
        let firstSut: CreditCardDetails = new CreditCardDetails(
            "John Smith",
            "2223003122003222",
            "12/30_ _ ___ __ __",
            "123");

        // Act
        let expiryMonth: string = firstSut.expiryMonthMM;
        let expiryYear: string = firstSut.expiryYearYY;

        // Assert
        expect(expiryMonth).toBe("12");
        expect(expiryYear).toBe("30");

        // Arrange
        let secondSut: CreditCardDetails = new CreditCardDetails(
            "John Smith",
            "2223003122003222",
            "12/30/_ _ ___ __ __",
            "123");

        // Act
        expiryMonth = secondSut.expiryMonthMM;
        expiryYear = secondSut.expiryYearYY;

        // Assert
        expect(expiryMonth).toBe("12");
        expect(expiryYear).toBe("30");
    });

    it('cardCCV should return the correct CCV numeric value.', () => {
        // Arrange
        let sut: CreditCardDetails = new CreditCardDetails(
            "John Smith",
            "2223003122003222",
            "12/30",
            "123__ __ __ __");

        // Act
        let ccv: string = sut.ccv;

        // Assert
        expect(ccv).toBe("123");
    });
});
