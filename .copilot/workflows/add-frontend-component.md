# Workflow: Add Frontend Component
**Estimated Time:** 30-45 minutes  
**Complexity:** Medium  
**Prerequisites:** React, TypeScript, shadcn/ui basics

---

## 📋 Overview

This workflow guides you through creating a complete React component with forms, API integration, and proper error handling.

**What You'll Create:**
1. Component with TypeScript types
2. Form with validation (React Hook Form + Zod)
3. API service integration (TanStack Query)
4. Loading/error/empty states
5. Responsive design (Tailwind CSS)

---

## ⏱️ Time-Boxed Steps

### Step 1: Define TypeScript Types (5 minutes)

**Location:** `Frontend/src/types/{entity}.ts`

**Template:**
```typescript
// Response DTO (from API)
export interface {Entity} {
  id: string;
  schoolId: string;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

// List item (subset for tables/lists)
export interface {Entity}ListItem {
  id: string;
  name: string;
  description?: string;
  isActive: boolean;
}

// Create request
export interface Create{Entity}Dto {
  name: string;
  description?: string;
}

// Update request
export interface Update{Entity}Dto {
  name?: string;
  description?: string;
  isActive?: boolean;
}

// API response with pagination
export interface {Entity}PagedResult {
  items: {Entity}[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}
```

**Checklist:**
- [ ] All types exported
- [ ] Optional fields marked with `?`
- [ ] Dates as strings (will parse later)
- [ ] Create/Update DTOs don't include id/schoolId

---

### Step 2: Create API Service (10 minutes)

**Location:** `Frontend/src/services/{entity}-service.ts`

**Template:**
```typescript
import { api } from '@/lib/api-client';
import type { 
  {Entity}, 
  {Entity}ListItem, 
  Create{Entity}Dto, 
  Update{Entity}Dto,
  {Entity}PagedResult 
} from '@/types/{entity}';

export const {entity}Service = {
  async getAll(params?: { 
    pageNumber?: number; 
    pageSize?: number; 
    searchTerm?: string;
  }): Promise<{Entity}PagedResult> {
    const response = await api.get<{Entity}PagedResult>('/{entities}', { params });
    return response.data;
  },

  async getAllSimple(): Promise<{Entity}ListItem[]> {
    const response = await api.get<{Entity}ListItem[]>('/{entities}/simple');
    return response.data;
  },

  async getById(id: string): Promise<{Entity}> {
    const response = await api.get<{Entity}>(`/{entities}/${id}`);
    return response.data;
  },

  async create(data: Create{Entity}Dto): Promise<{Entity}> {
    const response = await api.post<{Entity}>('/{entities}', data);
    return response.data;
  },

  async update(id: string, data: Update{Entity}Dto): Promise<{Entity}> {
    const response = await api.put<{Entity}>(`/{entities}/${id}`, data);
    return response.data;
  },

  async delete(id: string): Promise<void> {
    await api.delete(`/{entities}/${id}`);
  },
};
```

**Checklist:**
- [ ] All CRUD methods present
- [ ] Proper TypeScript types
- [ ] Correct HTTP methods (GET/POST/PUT/DELETE)
- [ ] Pagination support in getAll
- [ ] Returns typed responses

---

### Step 3: Create Validation Schema (5 minutes)

**Location:** `Frontend/src/schemas/{entity}-schema.ts`

**Template:**
```typescript
import { z } from 'zod';

export const create{Entity}Schema = z.object({
  name: z.string()
    .min(1, 'Name is required')
    .max(200, 'Name cannot exceed 200 characters')
    .regex(/^[a-zA-Z0-9\s]+$/, 'Name can only contain letters, numbers, and spaces'),
  
  description: z.string()
    .max(1000, 'Description cannot exceed 1000 characters')
    .optional()
    .or(z.literal('')),
});

export const update{Entity}Schema = z.object({
  name: z.string()
    .min(1, 'Name is required')
    .max(200, 'Name cannot exceed 200 characters')
    .optional(),
  
  description: z.string()
    .max(1000, 'Description cannot exceed 1000 characters')
    .optional()
    .or(z.literal('')),
  
  isActive: z.boolean().optional(),
});

export type Create{Entity}FormData = z.infer<typeof create{Entity}Schema>;
export type Update{Entity}FormData = z.infer<typeof update{Entity}Schema>;
```

**Checklist:**
- [ ] Required fields validated
- [ ] Max lengths match backend
- [ ] Regex patterns for format validation
- [ ] Optional fields properly typed
- [ ] Form data types exported

---

### Step 4: Create Form Component (15 minutes)

**Location:** `Frontend/src/components/{entities}/{entity}-form.tsx`

**Template:**
```typescript
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { useToast } from '@/hooks/use-toast';
import { {entity}Service } from '@/services/{entity}-service';
import { create{Entity}Schema, type Create{Entity}FormData } from '@/schemas/{entity}-schema';

interface {Entity}FormProps {
  {entity}Id?: string;
  onSuccess?: () => void;
}

export function {Entity}Form({ {entity}Id, onSuccess }: {Entity}FormProps) {
  const { toast } = useToast();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<Create{Entity}FormData>({
    resolver: zodResolver(create{Entity}Schema),
  });

  const mutation = useMutation({
    mutationFn: (data: Create{Entity}FormData) =>
      {entity}Id
        ? {entity}Service.update({entity}Id, data)
        : {entity}Service.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['{entities}'] });
      toast({
        title: 'Success',
        description: `{Entity} ${{{entity}Id ? 'updated' : 'created'}} successfully`,
      });
      onSuccess?.();
      navigate('/{entities}');
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to save {entity}',
        variant: 'destructive',
      });
    },
  });

  const onSubmit = (data: Create{Entity}FormData) => {
    mutation.mutate(data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      <div className="space-y-4">
        {/* Name Field */}
        <div className="space-y-2">
          <Label htmlFor="name">Name</Label>
          <Input
            id="name"
            {...register('name')}
            placeholder="Enter {entity} name"
            disabled={isSubmitting}
            className={errors.name ? 'border-destructive' : ''}
          />
          {errors.name && (
            <p className="text-sm text-destructive">{errors.name.message}</p>
          )}
        </div>

        {/* Description Field */}
        <div className="space-y-2">
          <Label htmlFor="description">Description</Label>
          <Textarea
            id="description"
            {...register('description')}
            placeholder="Enter description (optional)"
            disabled={isSubmitting}
            rows={4}
            className={errors.description ? 'border-destructive' : ''}
          />
          {errors.description && (
            <p className="text-sm text-destructive">{errors.description.message}</p>
          )}
        </div>
      </div>

      {/* Form Actions */}
      <div className="flex justify-end gap-4">
        <Button
          type="button"
          variant="outline"
          onClick={() => navigate('/{entities}')}
          disabled={isSubmitting}
        >
          Cancel
        </Button>
        <Button type="submit" disabled={isSubmitting}>
          {isSubmitting ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Saving...
            </>
          ) : (
            'Save {Entity}'
          )}
        </Button>
      </div>
    </form>
  );
}
```

**Checklist:**
- [ ] Form validation with Zod
- [ ] Loading state on submit
- [ ] Error messages displayed
- [ ] Cancel button navigation
- [ ] Success toast notification
- [ ] Query cache invalidation
- [ ] Disabled state during submission

---

### Step 5: Create List Component (20 minutes)

**Location:** `Frontend/src/components/{entities}/{entity}-list.tsx`

**Template:**
```typescript
import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Loader2, Plus, Pencil, Trash2, Search } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { useToast } from '@/hooks/use-toast';
import { {entity}Service } from '@/services/{entity}-service';

export function {Entity}List() {
  const navigate = useNavigate();
  const { toast } = useToast();
  const queryClient = useQueryClient();
  const [searchTerm, setSearchTerm] = useState('');
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [page, setPage] = useState(1);

  // Fetch data
  const { data, isLoading, error } = useQuery({
    queryKey: ['{entities}', page, searchTerm],
    queryFn: () => {entity}Service.getAll({ 
      pageNumber: page, 
      pageSize: 50,
      searchTerm 
    }),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });

  // Delete mutation
  const deleteMutation = useMutation({
    mutationFn: {entity}Service.delete,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['{entities}'] });
      toast({
        title: 'Success',
        description: '{Entity} deleted successfully',
      });
      setDeleteId(null);
    },
    onError: (error: Error) => {
      toast({
        title: 'Error',
        description: error.message || 'Failed to delete {entity}',
        variant: 'destructive',
      });
    },
  });

  const handleDelete = (id: string) => {
    setDeleteId(id);
  };

  const confirmDelete = () => {
    if (deleteId) {
      deleteMutation.mutate(deleteId);
    }
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <Card className="border-destructive">
        <CardContent className="pt-6">
          <p className="text-destructive">Failed to load {entities}. Please try again.</p>
        </CardContent>
      </Card>
    );
  }

  // Empty state
  if (!data?.items || data.items.length === 0) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-12">
          <Search className="h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-semibold mb-2">No {Entities} Found</h3>
          <p className="text-muted-foreground text-center mb-4">
            {searchTerm 
              ? `No results for "${searchTerm}"`
              : 'Get started by creating your first {entity}.'}
          </p>
          <Button onClick={() => navigate('/{entities}/new')}>
            <Plus className="mr-2 h-4 w-4" />
            Add {Entity}
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex-1 max-w-sm">
          <Input
            placeholder="Search {entities}..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full"
          />
        </div>
        <Button onClick={() => navigate('/{entities}/new')}>
          <Plus className="mr-2 h-4 w-4" />
          Add {Entity}
        </Button>
      </div>

      {/* Table */}
      <Card>
        <CardHeader>
          <CardTitle>{Entities}</CardTitle>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Description</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.items.map(({entity}) => (
                <TableRow key={{entity}.id}>
                  <TableCell className="font-medium">{{{entity}.name}}</TableCell>
                  <TableCell className="text-muted-foreground">
                    {{{entity}.description || '-'}}
                  </TableCell>
                  <TableCell>
                    <span
                      className={`inline-flex items-center rounded-full px-2 py-1 text-xs font-medium ${
                        {entity}.isActive
                          ? 'bg-green-50 text-green-700'
                          : 'bg-gray-50 text-gray-700'
                      }`}
                    >
                      {{{entity}.isActive ? 'Active' : 'Inactive'}}
                    </span>
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex justify-end gap-2">
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => navigate(`/{entities}/${{entity}.id}/edit`)}
                      >
                        <Pencil className="h-4 w-4" />
                      </Button>
                      <Button
                        variant="ghost"
                        size="icon"
                        onClick={() => handleDelete({entity}.id)}
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>

          {/* Pagination */}
          {data.totalPages > 1 && (
            <div className="flex items-center justify-between mt-4">
              <p className="text-sm text-muted-foreground">
                Page {data.pageNumber} of {data.totalPages} ({data.totalCount} total)
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(p => Math.max(1, p - 1))}
                  disabled={page === 1}
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => setPage(p => p + 1)}
                  disabled={page >= data.totalPages}
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={!!deleteId} onOpenChange={() => setDeleteId(null)}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Are you sure?</AlertDialogTitle>
            <AlertDialogDescription>
              This action cannot be undone. This will permanently delete the {entity}.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Cancel</AlertDialogCancel>
            <AlertDialogAction onClick={confirmDelete}>Delete</AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
```

**Checklist:**
- [ ] Search functionality
- [ ] Pagination controls
- [ ] Loading state (spinner)
- [ ] Error state (error card)
- [ ] Empty state (no data message)
- [ ] Edit/Delete actions
- [ ] Delete confirmation dialog
- [ ] Toast notifications
- [ ] Responsive design (mobile-friendly)

---

### Step 6: Create Routes (5 minutes)

**Location:** `Frontend/src/App.tsx` or routing file

```typescript
import { {Entity}List } from '@/components/{entities}/{entity}-list';
import { {Entity}Form } from '@/components/{entities}/{entity}-form';

// Add routes
<Routes>
  <Route path="/{entities}" element={<{Entity}List />} />
  <Route path="/{entities}/new" element={<{Entity}Form />} />
  <Route path="/{entities}/:id/edit" element={<{Entity}Form {entity}Id={params.id} />} />
</Routes>
```

**Checklist:**
- [ ] List route
- [ ] Create route
- [ ] Edit route with parameter

---

## ✅ Final Checklist

**TypeScript Types:**
- [ ] Entity interface defined
- [ ] Create/Update DTOs defined
- [ ] Paged result interface
- [ ] All types exported

**API Service:**
- [ ] CRUD methods implemented
- [ ] Proper HTTP methods
- [ ] TypeScript types used
- [ ] Error handling

**Validation:**
- [ ] Zod schema created
- [ ] Required fields validated
- [ ] Max lengths match backend
- [ ] Form data types exported

**Form Component:**
- [ ] React Hook Form integrated
- [ ] Zod validation resolver
- [ ] Loading/disabled states
- [ ] Error messages displayed
- [ ] Success/error toasts
- [ ] Cancel navigation

**List Component:**
- [ ] Search functionality
- [ ] Pagination
- [ ] Loading state
- [ ] Error state
- [ ] Empty state
- [ ] Edit/Delete actions
- [ ] Delete confirmation
- [ ] Responsive design

**Routes:**
- [ ] List route
- [ ] Create route
- [ ] Edit route

---

## 🚨 Common Mistakes to Avoid

1. **Not handling loading states** → Poor UX
2. **Missing error boundaries** → App crashes
3. **No empty state** → Confusing when no data
4. **Forgetting query invalidation** → Stale data
5. **No delete confirmation** → Accidental deletions
6. **Not disabling during submission** → Double submissions
7. **Missing responsive classes** → Broken mobile layout

---

## ⏱️ Time Breakdown

- Step 1 (TypeScript Types): 5 min
- Step 2 (API Service): 10 min
- Step 3 (Validation Schema): 5 min
- Step 4 (Form Component): 15 min
- Step 5 (List Component): 20 min
- Step 6 (Routes): 5 min
- **Total: 60 minutes**

With practice, you can complete this in **35-40 minutes**.

---

**Next Steps:**
- Add component tests (Vitest + React Testing Library)
- Add accessibility features (ARIA labels)
- Add keyboard navigation
- Optimize with React.memo if needed

**Related Files:**
- `.copilot/agents/frontend-agent.md`
- `.copilot/workflows/add-new-controller.md`
