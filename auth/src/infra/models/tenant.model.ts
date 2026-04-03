import mongoose, { Schema, HydratedDocument } from "mongoose";
import { ITenant } from "../../domain/interfaces/tenant.interface";

export type ITenantDocument = HydratedDocument<ITenant>;

const TenantSchema = new Schema<ITenant>(
  {
    _id: { type: String, required: true },
    name: { type: String, required: true, trim: true },
    isActive: { type: Boolean, default: true },
  },
  {
    timestamps: true,
    _id: false,
  },
);

export const TenantModel = mongoose.model<ITenant>("Tenant", TenantSchema);
