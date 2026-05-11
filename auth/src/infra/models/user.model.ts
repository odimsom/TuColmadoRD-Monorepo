import mongoose, { Schema, Document } from "mongoose";
import { IUser } from "../../domain/interfaces/user.interface";
import { Role } from "../../domain/enums/role.enums";
import { UserStatus } from "../../domain/enums/user-status.enum";

export interface IUserDocument extends Omit<IUser, "_id">, Document {}

const UserSchema = new Schema<IUserDocument>(
  {
    tenantId:              { type: String, required: true, index: true },
    email:                 { type: String, required: true, trim: true, lowercase: true },
    password:              { type: String, required: true },
    firstName:             { type: String, default: null },
    lastName:              { type: String, default: null },
    role:                  { type: String, enum: Object.values(Role), required: true },
    status:                { type: String, enum: Object.values(UserStatus), default: UserStatus.PENDING_VERIFICATION },
    verificationCode:      { type: String, default: null },
    verificationCodeExpiry: { type: Date, default: null },
  },
  { timestamps: true },
);

UserSchema.index({ email: 1, tenantId: 1 }, { unique: true });

export const UserModel = mongoose.model<IUserDocument>("User", UserSchema);
