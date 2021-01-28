using MercuryMartAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Helpers
{
    public class Utils
    {
        public const int Administrator = 1;
        public const int Customer = 2;

        //STATUS CODES
        public const int Success = 0;
        public const int MailFailure = 1;
        public const int NotFound = 2;
        public const int ObjectNull = 3;
        public const int SaveError = 4;
        public const int SaveNoRowAffected = 5;
        public const int NotSucceeded = 6;
        public const int ObjectExists = 7;
        public const int BadRequest = 8;
        public const int SignInError = 9;
        public const int EmailAlreadyConfirmed = 10;
        public const int PreviousPasswordStorageError = 11;
        public const int NewPasswordError = 12;
        public const int InvalidUserType = 13;
        public const int RoleAssignmentError = 14;
        public const int InvalidFileSize = 15;
        public const int CloudinaryFileUploadError = 16;
        public const int CloudinaryFileDeleteError = 17;
        public const int CloudinaryDeleteError = 18;
        public const int UserNotAllowed = 19;
        public const int UserClaimNotFound = 20;

        // STATUS MESSAGES
        public const string StatusMessageSuccess = "Request Successful";
        public const string StatusMessageMailFailure = "Object could not send Mail";
        public const string StatusMessageNotFound = "Object was not Found";
        public const string StatusMessageObjectNull = "Object is Empty";
        public const string StatusMessageSaveError = "Object was unable to Save";
        public const string StatusMessageSaveNoRowAffected = "";
        public const string StatusMessageNotSucceeded = "Action on this Object did not Succeed";
        public const string StatusMessageObjectExists = "Object already Exists";
        public const string StatusMessageBadRequest = "You cannot perform the Operation you are trying to do";
        public const string StatusMessageSignInError = "Error Signing In. Incorrect Email/Password";
        public const string StatusMessageEmailAlreadyConfirmed = "The Email Address has already been Confirmed by MercuryMart";
        public const string StatusMessagePreviousPasswordStorageError = "Previous Password is the same as New Password";
        public const string StatusMessageNewPasswordError = "";
        public const string StatusMessageInvalidUserType = "The User Type is Invalid";
        public const string StatusMessageRoleAssignmentError = "Unable to Assign Role";
        public const string StatusMessageInvalidFileSize = "The Size of the Uploaded File is Invalid";
        public const string StatusMessageCloudinaryFileUploadError = "An Error Occured While Uploading the File";
        public const string StatusMessageCloudinaryFileDeleteError = "An Error Occured While Deleting the File";
        public const string StatusMessageCloudinaryDeleteError = "An Error Occured while Deleting files from Cloudinary";
        public const string StatusMessageUserNotAllowed = "User Not Allowed Because His/Her Role doesnt have Access Level to the Functionality";
        public const string StatusMessageUserClaimNotFound = "User Claim was Not Found";

        //ROLES
        public const string SystemAdminRole = "SystemAdmin";
        public const string NormalAdminRoe = "NormalAdmin";
        public const string CustomerRole = "Customer";

        //CLAIM EXTRA TYPES
        public const string ClaimType_UserType = "UserType";
        public const string ClaimType_UserEmail = "UserEmail";

        //ORDER/DELIVERY STATUS
        public const int CurrentOrderNumberStatus_Pending = 1;
        public const int CurrentOrderNumberStatus_Completed = 2;

        public const int UserClaim_Null = 0;

        public const double Zero_Price = 0.0;
    }
}