using System.Collections.Generic;

using Messenger.Model;
using Messenger.Model.Enums;

namespace Messenger.DataLayer
{
    public interface IChatsRepository
    {
        /// <summary>
        /// Create chat using the first member as the creator
        /// </summary>
        /// <param name="usersIds"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        Chat CreateGroupChat(IEnumerable<int> usersIds, string title);
        
        /// <summary>
        /// Equal to creating GroupChat with 'title' named as concatenation of users' names.
        /// </summary>
        /// <param name="userId1"></param>
        /// <param name="userId2"></param>
        /// <returns></returns>
        Chat CreateDialog(int userId1, int userId2);

        Chat GetChat(int chatId);
        /// <summary>
        /// Get userIdList by chatId
        /// </summary>
        /// <param name="chatId"></param>
        /// <returns></returns>
        IEnumerable<User> GetChatUsers(int chatId);

        /// <summary>
        /// Get chatIdList by userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<Chat> GetUserChats(int userId);
        void DeleteChat(int chatId);

        void SetCreator(int chatId, int newCreatorUserId);
        void AddUser(int chatId, int userId);
        void KickUser(int chatId, int userId);

        void AddUsers(int chatId, IEnumerable<int> addeddUsersIds);
        void KickUsers(int chatId, IEnumerable<int> kickedUsersIds);

    }
}