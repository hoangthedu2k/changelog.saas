import { SubscriptionPlan } from "../../shared/enums/SubscriptionPlan";
import { BaseEntity } from "./baseentity.model";

export interface User extends BaseEntity {
    email: string;
    passwordHash: string;
    displayName?: string;
    plan?: SubscriptionPlan | string;
}
export interface RegisterRequest {  
    email: string;
    password: string;
    displayName?: string;
}

export interface LoginRequest {
    email: string;
    password: string;
}
export interface LoginResponse {
    token: string;
    user: User;
}
