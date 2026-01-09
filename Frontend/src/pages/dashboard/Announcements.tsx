import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Plus, Bell, Calendar, User, Megaphone, Edit, Trash2, Eye, Search } from "lucide-react";
import { useAnnouncement } from "@/hooks/useAnnouncement";
import AddAnnouncementPopup from "@/popups/announcements/AddAnnouncementPopup";
import EditAnnouncementPopup from "@/popups/announcements/EditAnnouncementPopup";
import ViewAnnouncementPopup from "@/popups/announcements/ViewAnnouncementPopup";
import DeleteAnnouncementPopup from "@/popups/announcements/DeleteAnnouncementPopup";

const Announcements: React.FC = () => {
  const { data: announcements, isLoading, error, addAnnouncement, editAnnouncement, deleteAnnouncement } = useAnnouncement();

  const [searchTerm, setSearchTerm] = useState("");
  const [selectedAnnouncement, setSelectedAnnouncement] = useState<any | null>(null);
  const [modal, setModal] = useState<"add" | "edit" | "view" | "delete" | null>(null);
  const [isOpen, setIsOpen] = useState(false);

  const openModal = (type: typeof modal, announcement?: any) => {
    setModal(type);
    setSelectedAnnouncement(announcement || null);
    setIsOpen(true);
  };

  const closeModal = () => {
    setModal(null);
    setSelectedAnnouncement(null);
    setIsOpen(false);
  };

  const handleAdd = async (newAnnouncement: any) => {
    await addAnnouncement({ newAnnouncement });
    closeModal();
  };

  const handleEdit = async (updatedAnnouncement: any) => {
    if (!selectedAnnouncement) return;
    await editAnnouncement({ updatedAnnouncement: { ...selectedAnnouncement, ...updatedAnnouncement } });
    closeModal();
  };

  const handleDelete = async (id: string) => {
    await deleteAnnouncement({ id });
    closeModal();
  };

  const filteredAnnouncements = announcements?.filter((a) =>
    a.title.toLowerCase().includes(searchTerm.toLowerCase())
  );

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h2 className="text-2xl sm:text-3xl font-bold text-gray-900">Announcements</h2>
          <p className="text-gray-600 mt-2">Stay updated with school news and events</p>
        </div>
        <Button className="flex items-center space-x-2 w-full sm:w-auto" onClick={() => openModal("add")}>
          <Plus className="h-4 w-4" />
          <span>New Announcement</span>
        </Button>
      </div>

      <div className="relative mb-6">
        <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-4 w-4" />
        <input
          type="text"
          placeholder="Search announcements..."
          className="pl-10 pr-4 py-2 border border-gray-300 rounded-lg w-full"
          value={searchTerm}
          onChange={(e) => setSearchTerm(e.target.value)}
        />
      </div>

      {Array.isArray(filteredAnnouncements) && filteredAnnouncements.length > 0 ? (
        <div className="space-y-4">
          {filteredAnnouncements.map((announcement) => (
            <Card key={announcement.id} className="hover:shadow-lg transition-shadow">
              <CardHeader>
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2">
                  <CardTitle className="flex items-center space-x-2">
                    <Bell className="h-5 w-5 text-primary-600" />
                    <span className="text-lg">{announcement.title}</span>
                  </CardTitle>
                  <div className="flex space-x-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => openModal("view", announcement)}
                    >
                      <Eye className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => openModal("edit", announcement)}
                    >
                      <Edit className="h-4 w-4" />
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => openModal("delete", announcement)}
                    >
                      <Trash2 className="h-4 w-4 text-red-500" />
                    </Button>
                  </div>
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                <p className="text-gray-700">{announcement.detail}</p>
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-2 text-sm text-gray-500">
                  <div className="flex items-center space-x-2">
                    <User className="h-4 w-4" />
                    <span>{announcement.announcedBy}</span>
                  </div>
                  <div className="flex items-center space-x-2">
                    <Calendar className="h-4 w-4" />
                    <span>{new Date(announcement.date).toLocaleDateString()}</span>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center text-center py-16 px-6 mt-12">
          <div className="bg-gray-200 rounded-full p-4 mb-4">
            <Megaphone className="h-12 w-12 text-primary-700" />
          </div>
          <h2 className="text-2xl sm:text-3xl font-bold text-primary-800 mb-2">
            No Announcements Found
          </h2>
          <p className="text-gray-600 text-base sm:text-lg max-w-md mb-6">
            📢 You haven’t posted any announcements yet. Keep your students informed by creating one now.
          </p>
          <Button
            onClick={() => openModal("add")}
            className="flex items-center space-x-2"
          >
            <Plus className="h-4 w-4" />
            <span>Create Announcement</span>
          </Button>
        </div>
      )}

      {/* Modals */}
      {modal === "add" && (
        <AddAnnouncementPopup
          isOpen={isOpen}
          onClose={closeModal}
          onSubmit={handleAdd}
        />
      )}
      {modal === "edit" && selectedAnnouncement && (
        <EditAnnouncementPopup
          isOpen={isOpen}
          announcementData={selectedAnnouncement}
          onClose={closeModal}
          onSubmit={handleEdit}
        />
      )}
      {modal === "view" && selectedAnnouncement && (
        <ViewAnnouncementPopup
          isOpen={isOpen}
          announcementData={selectedAnnouncement}
          onClose={closeModal}
        />
      )}
      {modal === "delete" && selectedAnnouncement && (
        <DeleteAnnouncementPopup
          isOpen={isOpen}
          announcementData={selectedAnnouncement}
          onClose={closeModal}
          onConfirm={() => handleDelete(selectedAnnouncement.id)}
        />
      )}
    </div>
  );
};

export default Announcements;
