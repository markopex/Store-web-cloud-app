export class Checkout {
    address: string = "";
    comment: string = "";
    paymentMethod: string = "castatdelivery";
}

export class OrderCreatedSuccessfullyDto {
    orderId: string;
    redirectUrl?: string | null;

    constructor(orderId: string, redirectUrl?: string | null) {
        this.orderId = orderId;
        this.redirectUrl = redirectUrl;
    }
}

