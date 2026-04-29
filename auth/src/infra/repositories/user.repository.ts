import { IUser } from "../../domain/interfaces/user.interface";
import { UserModel } from "../models/user.model";

type UserDoc = Omit<IUser, "_id"> & { _id: unknown };

const toIUser = (doc: UserDoc): IUser =>
  ({
    ...doc,
    _id: String(doc._id),
  }) as IUser;

export class UserRepository {
  async findByEmailAndTenant(
    email: string,
    tenantId: string,
  ): Promise<IUser | null> {
    const doc = await UserModel.findOne({
      email,
      tenantId,
      isActive: true,
    }).lean();
    return doc ? toIUser(doc as UserDoc) : null;
  }

  async findByEmail(email: string): Promise<IUser | null> {
    const doc = await UserModel.findOne({
      email,
      isActive: true,
    }).lean();
    return doc ? toIUser(doc as UserDoc) : null;
  }

  async findById(id: string, tenantId: string): Promise<IUser | null> {
    const doc = await UserModel.findOne({
      _id: id,
      tenantId,
      isActive: true,
    }).lean();
    return doc ? toIUser(doc as UserDoc) : null;
  }

  async create(data: Omit<IUser, "_id" | "createdAt">): Promise<IUser> {
    const user = await UserModel.create(data);
    return toIUser(user.toObject() as UserDoc);
  }

  async delete(id: string, tenantId: string): Promise<void> {
    await UserModel.deleteOne({ _id: id, tenantId });
  }

  async existsByEmailAndTenant(
    email: string,
    tenantId: string,
  ): Promise<boolean> {
    const count = await UserModel.countDocuments({ email, tenantId });
    return count > 0;
  }

  async findAllByTenant(tenantId: string): Promise<IUser[]> {
    const docs = await UserModel.find({ tenantId }).sort({ createdAt: -1 }).lean();
    return docs.map(d => toIUser(d as UserDoc));
  }

  async updateById(id: string, tenantId: string, data: Partial<Pick<IUser, "firstName" | "lastName" | "role" | "isActive">>): Promise<IUser | null> {
    const doc = await UserModel.findOneAndUpdate(
      { _id: id, tenantId },
      { $set: data },
      { new: true, lean: true },
    );
    return doc ? toIUser(doc as UserDoc) : null;
  }

  async deactivate(id: string, tenantId: string): Promise<void> {
    await UserModel.updateOne({ _id: id, tenantId }, { $set: { isActive: false } });
  }

  async activate(id: string, tenantId: string): Promise<void> {
    await UserModel.updateOne({ _id: id, tenantId }, { $set: { isActive: true } });
  }
}
