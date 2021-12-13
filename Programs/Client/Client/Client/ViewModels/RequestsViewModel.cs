using Caliburn.Micro;
using CarCRUD.DataModels;
using CarCRUD.Users;
using System.Collections.Generic;
using System.Windows;

namespace CarCRUD.ViewModels
{
    class RequestsViewModel : Screen
    {
        #region Properties
        private UserRequest selectedRequest;

        public UserRequest SelectedRequest
        {
            get
            {
                return selectedRequest;
            }
            set
            {
                selectedRequest = value;
                NotifyOfPropertyChange(() => selectedRequest);
            }
        }
        public List<UserRequest> Requests
        {
            get
            {
                return UserController.user.adminResponseData.requests;
            }
            set
            {
                NotifyOfPropertyChange(() => UserController.user.adminResponseData.requests);
            }
        }
        #endregion

        #region Butten Events
        public void Accept()
        {
            if (SelectedRequest == null)
                return;

            UserActionHandler.RequestAnswerRequest(true, SelectedRequest.ID);
        }

        public void Dismiss()
        {
            if (SelectedRequest == null)
                return;

            UserActionHandler.RequestAnswerRequest(false, SelectedRequest.ID);
        }
        #endregion
    }
}
