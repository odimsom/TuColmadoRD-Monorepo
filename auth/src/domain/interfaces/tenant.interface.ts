export type SubscriptionStatus = 'active' | 'trialing' | 'expired';

export interface ITenant {
  _id: string;
  name: string;
  isActive: boolean;
  subscriptionStatus: SubscriptionStatus;
  createdAt: Date;
}
