import React from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Calendar, Megaphone, User } from 'lucide-react';

interface ViewAnnouncementPopupProps {
  isOpen: boolean;
  onClose: () => void;
  announcementData: any;
}

const getPriorityStyle = (priority: 'high' | 'medium' | 'low') => {
  switch (priority) {
    case 'high':
      return 'bg-red-100 text-red-800';
    case 'medium':
      return 'bg-yellow-100 text-yellow-800';
    case 'low':
      return 'bg-green-100 text-green-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
};

const ViewAnnouncementPopup: React.FC<ViewAnnouncementPopupProps> = ({
  isOpen,
  onClose,
  announcementData,
}) => {
  if (!announcementData) return null;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Megaphone className="h-5 w-5 text-primary-600" />
            {announcementData.title}
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-6">
          <div className="text-sm text-gray-700">
            {announcementData.detail}
          </div>

          <div className="grid grid-cols-2 gap-4 text-sm text-muted-foreground">
            <div className="flex items-center gap-2">
              <User className="h-4 w-4" />
              <span className="font-medium">Announced By:</span>
              <span>{announcementData.announcedBy}</span>
            </div>

            <div className="flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              <span className="font-medium">Date:</span>
              <span>
                {new Date(announcementData.date).toLocaleDateString()}
              </span>
            </div>
          </div>

          {announcementData.priority && (
            <div className="flex items-center gap-2">
              <span className="text-sm font-medium">Priority:</span>
              <Badge
                className={`${getPriorityStyle(
                  announcementData.priority
                )} text-xs px-2 py-1`}
              >
                {announcementData.priority.toUpperCase()}
              </Badge>
            </div>
          )}
        </div>

        <DialogFooter>
          <Button onClick={onClose}>Close</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export default ViewAnnouncementPopup;
