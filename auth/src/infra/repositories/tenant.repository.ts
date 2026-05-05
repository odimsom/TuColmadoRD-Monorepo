import { ITenant } from "../../domain/interfaces/tenant.interface";
import { TenantModel } from "../models/tenant.model";

export class TenantRepository {
  async findById(id: string): Promise<ITenant | null> {
    return TenantModel.findOne({ _id: id, isActive: true }).lean();
  }

  async create(data: Omit<ITenant, "createdAt">): Promise<ITenant> {
    const tenant = await TenantModel.create(data);
    return tenant.toObject();
  }

  async delete(id: string): Promise<void> {
    await TenantModel.deleteOne({ _id: id });
  }

  async exists(id: string): Promise<boolean> {
    const count = await TenantModel.countDocuments({ _id: id });
    return count > 0;
  }
}
