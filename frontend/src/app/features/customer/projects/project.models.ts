export interface ProjectDto {
  id: string;
  name: string;
  budget: number;
  status: 'Draft' | 'Active' | 'Completed' | 'Cancelled' | string;
  contractorUserId?: string | null;
  createdAt: string;
}

export interface MilestoneDto {
  id: string;
  projectId: string;
  title: string;
  amount: number;
  dueDate: string;
  status: 'Pending' | 'Completed' | 'Approved' | string;
}

export interface ExpenseDto {
  id: string;
  projectId: string;
  title: string;
  amount: number;
  receiptUrl?: string | null;
  createdAt: string;
}

export interface PhotoDto {
  id: string;
  projectId: string;
  url: string;
  caption?: string | null;
  createdAt: string;
}
