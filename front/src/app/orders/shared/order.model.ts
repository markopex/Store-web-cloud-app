import { OrderItem } from "./order-item.model";

export class Order {
    id: number;
    customerUsername: string;
    orderDetails: OrderItem[];
    comment: string;
    address: string;
    price: number;
    utcTimeOrderCreated: number;
    uTCTimeDeliveryStarted: number;
    utcTimeDeliveryExpected: number;
}