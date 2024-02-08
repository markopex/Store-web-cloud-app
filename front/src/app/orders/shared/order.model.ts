import { OrderItem } from "./order-item.model";

export class Order {
    id: string;
    customerUsername: string;
    orderDetails: OrderItem[];
    comment: string;
    status: number;
    address: string;
    price: number;
    utcTimeOrderCreated: number;
    uTCTimeDeliveryStarted: number;
    utcTimeDeliveryExpected: number;
    paymentMethod: number;
}

export class CaptureOrderDto {
    OrderId: string;

    constructor(OrderId: string) {
        this.OrderId = OrderId;
    }
}

export class CancelOrderDto {
    OrderId: string;

    constructor(OrderId: string) {
        this.OrderId = OrderId;
    }
}
